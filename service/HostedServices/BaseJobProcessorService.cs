using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using WhatsAppCampaignManager.Constants;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.Models;

namespace WhatsAppCampaignManager.HostedServices
{
    public abstract class BaseJobProcessorService : BackgroundService
    {
        protected readonly IServiceScopeFactory _scopeFactory;
        protected readonly ILogger _logger;
        protected readonly ConcurrentDictionary<int, SemaphoreSlim> _userLocks;
        protected readonly int _maxConcurrencyPerUser;
        protected readonly int _maxUsersProcessing;
        protected readonly TimeSpan _processingInterval;

        protected BaseJobProcessorService(
            IServiceScopeFactory scopeFactory, 
            ILogger logger,
            int maxConcurrencyPerUser = AppConst.JobSettings.MAX_CONCURRENT_JOBS_PER_USER,
            int maxUsersProcessing = AppConst.JobSettings.MAX_USERS_PROCESSING,
            int processingIntervalMinutes = AppConst.JobSettings.JOB_PROCESSING_INTERVAL_MINUTES)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _userLocks = new ConcurrentDictionary<int, SemaphoreSlim>();
            _maxConcurrencyPerUser = maxConcurrencyPerUser;
            _maxUsersProcessing = maxUsersProcessing;
            _processingInterval = TimeSpan.FromMinutes(processingIntervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{ServiceName} started with max {MaxConcurrency} jobs per user", 
                GetType().Name, _maxConcurrencyPerUser);

            await CloseRunningJobsWhenStart(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingJobsWithUserLimits(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("{ServiceName} is stopping", GetType().Name);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in {ServiceName}", GetType().Name);
                }
                finally
                {
                    await Task.Delay(_processingInterval, stoppingToken);
                }
            }
        }

        protected virtual async Task CloseRunningJobsWhenStart(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var runningJobs = await context.AppJobs
                    .Where(j => j.Status == AppConst.JobStatus.RUNNING)
                    .ToListAsync(stoppingToken);

                if (runningJobs.Any())
                {
                    _logger.LogWarning("Found {Count} running jobs on startup, marking as failed", runningJobs.Count);
                    
                    foreach (var job in runningJobs)
                    {
                        job.Status = AppConst.JobStatus.FAILED;
                        job.CompletedAt = DateTime.Now;
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing running jobs on startup");
            }
        }

        protected virtual async Task ProcessPendingJobsWithUserLimits(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get all pending jobs that are ready to run
            var pendingJobs = await context.AppJobs
                .Include(j => j.Message)
                .Include(j => j.Instance)
                .Include(j => j.CreatedByUser)
                .Where(j => j.Status == AppConst.JobStatus.PENDING && 
                           (j.ScheduledAt == null || j.ScheduledAt <= DateTime.Now))
                .AsNoTracking()
                .ToListAsync(stoppingToken);

            if (!pendingJobs.Any())
            {
                return;
            }

            // Group jobs by user
            var jobGroups = pendingJobs
                .GroupBy(j => j.CreatedByUserId)
                .Take(_maxUsersProcessing)
                .ToList();

            _logger.LogInformation("Processing jobs for {UserCount} users, total {JobCount} pending jobs", 
                jobGroups.Count, pendingJobs.Count);

            foreach (var group in jobGroups)
            {
                var userId = group.Key;
                var userJobs = group.OrderBy(j => j.CreatedAt).ToList();

                // Get or create user semaphore
                var semaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(_maxConcurrencyPerUser));

                foreach (var job in userJobs)
                {
                    if (semaphore.CurrentCount == 0)
                    {
                        // User đã đủ jobs đang chạy
                        _logger.LogDebug("User {UserId} has reached max concurrent jobs limit", userId);
                        break;
                    }

                    // Mark job as running
                    await MarkJobAsRunning(job.Id, stoppingToken);

                    // Process job asynchronously
                    _ = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(stoppingToken);
                        try
                        {
                            await ProcessSingleJobAsync(job, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Job {JobId} failed for user {UserId}", job.Id, userId);
                            await MarkJobAsFailed(job.Id, ex.Message, stoppingToken);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, stoppingToken);
                }
            }
        }

        protected virtual async Task MarkJobAsRunning(int jobId, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var job = await context.AppJobs.FindAsync(new object[] { jobId }, stoppingToken);
                if (job != null && job.Status == AppConst.JobStatus.PENDING)
                {
                    job.Status = AppConst.JobStatus.RUNNING;
                    job.StartedAt = DateTime.Now;
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking job {JobId} as running", jobId);
            }
        }

        protected virtual async Task MarkJobAsCompleted(int jobId, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var job = await context.AppJobs.FindAsync(new object[] { jobId }, stoppingToken);
                if (job != null)
                {
                    job.Status = AppConst.JobStatus.COMPLETED;
                    job.CompletedAt = DateTime.Now;
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking job {JobId} as completed", jobId);
            }
        }

        protected virtual async Task MarkJobAsFailed(int jobId, string errorMessage, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var job = await context.AppJobs.FindAsync(new object[] { jobId }, stoppingToken);
                if (job != null)
                {
                    job.Status = AppConst.JobStatus.FAILED;
                    job.CompletedAt = DateTime.Now;
                    await context.SaveChangesAsync(stoppingToken);

                    // Log error
                    var jobLog = new AppJobLog
                    {
                        JobId = jobId,
                        LogLevel = "Error",
                        Message = "Job failed",
                        Details = errorMessage,
                        CreatedAt = DateTime.Now
                    };
                    context.AppJobLogs.Add(jobLog);
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking job {JobId} as failed", jobId);
            }
        }

        protected abstract Task ProcessSingleJobAsync(AppJob job, CancellationToken stoppingToken);

        public override void Dispose()
        {
            // Dispose all semaphores
            foreach (var semaphore in _userLocks.Values)
            {
                semaphore?.Dispose();
            }
            _userLocks.Clear();
            base.Dispose();
        }
    }
}
