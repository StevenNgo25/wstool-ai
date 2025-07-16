using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WhatsAppCampaignManager.Constants;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Models;
using WhatsAppCampaignManager.Extensions;
using Newtonsoft.Json;

namespace WhatsAppCampaignManager.Services.Implements
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JobService> _logger;

        public JobService(ApplicationDbContext context, ILogger<JobService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<JobDto?> CreateJobAsync(CreateJobDto createJobDto, int userId)
        {
            try
            {
                // Validate message exists and user has access
                var message = await _context.AppMessages
                    .Include(m => m.Instance)
                    .FirstOrDefaultAsync(m => m.Id == createJobDto.MessageId);

                if (message == null)
                {
                    _logger.LogWarning("Message with ID {MessageId} not found", createJobDto.MessageId);
                    return null;
                }

                // Validate user has access to the message's instance
                var hasAccess = await _context.AppUserInstances
                    .AnyAsync(ui => ui.UserId == userId && ui.InstanceId == createJobDto.InstanceId);

                if (!hasAccess)
                {
                    _logger.LogWarning("User {UserId} does not have access to instance {InstanceId}", userId, createJobDto.InstanceId);
                    return null;
                }

                //// Check user's pending/running jobs limit
                //var userActiveJobs = await _context.AppJobs
                //    .CountAsync(j => j.CreatedByUserId == userId && 
                //               (j.Status == AppConst.JobStatus.PENDING || j.Status == AppConst.JobStatus.RUNNING));

                //if (userActiveJobs >= 10) // Max 10 pending/running jobs per user
                //{
                //    _logger.LogWarning("User {UserId} has reached maximum active jobs limit", userId);
                //    return null;
                //}

                // Prepare target data
                string? targetData = null;
                if (createJobDto.JobType == "SendToGroups" && createJobDto.GroupIds?.Any() == true)
                {
                    targetData = System.Text.Json.JsonSerializer.Serialize(createJobDto.GroupIds);
                }
                else if (createJobDto.JobType == "SendToUsers" && createJobDto.PhoneNumbers?.Any() == true)
                {
                    targetData = System.Text.Json.JsonSerializer.Serialize(createJobDto.PhoneNumbers);
                }

                var job = new AppJob
                {
                    JobType = createJobDto.JobType,
                    MessageId = createJobDto.MessageId,
                    InstanceId = createJobDto.InstanceId, // Use message's instance
                    CreatedByUserId = userId,
                    ScheduledAt = createJobDto.ScheduledAt,
                    TargetData = targetData,
                    Status = AppConst.JobStatus.PENDING
                };

                _context.AppJobs.Add(job);
                await _context.SaveChangesAsync();

                return await GetJobByIdAsync(job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job");
                return null;
            }
        }

        public async Task<PaginatedResponse<JobDto>> GetJobsAsync(PaginationRequest request, int? userId = null)
        {
            var query = _context.AppJobs
                .Include(j => j.Message)
                .Include(j => j.Instance)
                .Include(j => j.CreatedByUser)
                .Include(j => j.SentMessages)
                .Include(j => j.JobLogs.OrderByDescending(l => l.CreatedAt).Take(10)) // Latest 10 logs
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(j => j.CreatedByUserId == userId.Value);
            }

            var jobQuery = query.Select(j => new JobDto
            {
                Id = j.Id,
                JobType = j.JobType,
                Status = j.Status,
                InstanceName = j.Instance.Name,
                CreatedByUserName = j.CreatedByUser.Username,
                ScheduledAt = j.ScheduledAt,
                StartedAt = j.StartedAt,
                CompletedAt = j.CompletedAt,
                CreatedAt = j.CreatedAt,
                TotalSentMessages = j.SentMessages.Count,
                MessageTitle = j.Message.TextContent ?? string.Empty,
                SuccessfulMessages = j.SentMessages.Count(sm => sm.Status == "Delivered" || sm.Status == "Read"),
                FailedMessages = j.SentMessages.Count(sm => sm.Status == "Failed"),
                Logs = j.JobLogs.Select(l => new JobLogDto
                {
                    Id = l.Id,
                    LogLevel = l.LogLevel,
                    Message = l.Message,
                    Details = l.Details,
                    CreatedAt = l.CreatedAt
                }).ToList()

            });

            // Apply search
            jobQuery = jobQuery.ApplySearch(request.Search, 
                x => x.MessageTitle,
                x => x.InstanceName,
                x => x.CreatedByUserName);

            // Apply sorting
            jobQuery = jobQuery.ApplySort(request.SortBy, request.SortDirection);

            // If no sort specified, default to CreatedAt desc
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                jobQuery = jobQuery.OrderByDescending(x => x.CreatedAt);
            }

            return await jobQuery.ToPaginatedResponseAsync(request);
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _context.AppJobs
                .Include(j => j.Message)
                .Include(j => j.Instance)
                .Include(j => j.CreatedByUser)
                .Include(j => j.SentMessages.OrderByDescending(l => l.SentAt))
                //.Include(j => j.JobLogs.OrderByDescending(l => l.CreatedAt))
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job != null)
            {
                job.JobLogs = await _context.AppJobLogs
                    .Where(l => l.JobId == job.Id)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(100)
                    .ToListAsync();
            }

            if (job == null) return null;

            var assignGroupIds = job.JobType == "SendToGroups" && job.TargetData != null
                    ? JsonConvert.DeserializeObject<List<int>>(job.TargetData) : null;
            var assignedGroups = await _context.AppGroups.Where(g => assignGroupIds.Contains(g.Id)).ToListAsync();

            return new JobDto
            {
                Id = job.Id,
                JobType = job.JobType,
                Status = job.Status,
                InstanceName = job.Instance.Name,
                CreatedByUserName = job.CreatedByUser.Username,
                ScheduledAt = job.ScheduledAt,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                CreatedAt = job.CreatedAt,
                MessageTitle = job.Message.TextContent ?? string.Empty,
                TotalSentMessages = job.SentMessages.Count,
                SuccessfulMessages = job.SentMessages.Count(sm => sm.Status == "Delivered" || sm.Status == "Read"),
                FailedMessages = job.SentMessages.Count(sm => sm.Status == "Failed"),
                Logs = job.JobLogs.Select(l => new JobLogDto
                {
                    Id = l.Id,
                    LogLevel = l.LogLevel,
                    Message = l.Message,
                    Details = l.Details,
                    CreatedAt = l.CreatedAt
                }).ToList(),
                SentMessages = job.SentMessages.Select(l => new JobSentMessageDto
                {
                    Id = l.Id,
                    JobId = l.JobId,
                    RecipientId = l.RecipientId,
                    RecipientType = l.RecipientType,
                    WhapiMessageId = l.WhapiMessageId,
                    Status = l.Status,
                    ErrorMessage = l.ErrorMessage,
                    SentAt = l.SentAt,
                    DeliveredAt = l.DeliveredAt,
                    ReadAt = l.ReadAt,
                    LastValidatedAt = l.LastValidatedAt
                }).ToList(),
                AssignedGroups = assignedGroups.Select(s=> new GroupDto
                {
                    Id = s.Id,
                    GroupId = s.GroupId,
                    Name = s.Name,
                    Description = s.Description,
                    ParticipantCount = s.ParticipantCount
                }).ToList(),
                TargetPhoneNumbers = job.JobType == "SendToUsers" && job.TargetData != null ? JsonConvert.DeserializeObject<List<string>>(job.TargetData) : null
            };
        }

        public async Task<bool> UpdateJobStatusAsync(int jobId, string status)
        {
            try
            {
                var job = await _context.AppJobs.FindAsync(jobId);
                if (job == null) return false;

                job.Status = status;
                
                if (status == AppConst.JobStatus.RUNNING && job.StartedAt == null)
                {
                    job.StartedAt = DateTime.Now;
                }
                else if ((status == AppConst.JobStatus.COMPLETED || status == AppConst.JobStatus.FAILED) && job.CompletedAt == null)
                {
                    job.CompletedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job status");
                return false;
            }
        }

        public async Task<bool> RestartJobAsync(int jobId, int userId, string userRole)
        {
            try
            {
                var job = await _context.AppJobs.FindAsync(jobId);
                if (job == null) return false;

                // Check permissions
                if (job.CreatedByUserId != userId && userRole != "Admin")
                {
                    _logger.LogWarning("User {UserId} attempted to restart job {JobId} without permission", userId, jobId);
                    return false;
                }

                // Only allow restart for completed or failed jobs
                if (job.Status != AppConst.JobStatus.COMPLETED && job.Status != AppConst.JobStatus.FAILED)
                {
                    _logger.LogWarning("Cannot restart job {JobId} with status {Status}", jobId, job.Status);
                    return false;
                }

                // Reset job status and timestamps
                job.Status = AppConst.JobStatus.PENDING;
                job.StartedAt = null;
                job.CompletedAt = null;

                // Clear previous sent messages and logs
                var sentMessages = await _context.AppSentMessages.Where(sm => sm.JobId == jobId).ToListAsync();
                _context.AppSentMessages.RemoveRange(sentMessages);

                await _context.SaveChangesAsync();

                // Log restart
                await LogJobMessageAsync(jobId, "Info", "Job restarted by user", $"Restarted by {userRole}: {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restarting job {JobId}", jobId);
                return false;
            }
        }

        public async Task<bool> DeleteJobAsync(int id, int userId)
        {
            try
            {
                var job = await _context.AppJobs.FindAsync(id);
                if (job == null || job.CreatedByUserId != userId) return false;

                if (job.Status == AppConst.JobStatus.RUNNING)
                {
                    _logger.LogWarning("Cannot delete running job {JobId}", id);
                    return false;
                }

                _context.AppJobs.Remove(job);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job");
                return false;
            }
        }

        public async Task<bool> BulkDeleteJobsAsync(List<int> jobIds, int userId, string userRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var jobs = await _context.AppJobs
                    .Where(j => jobIds.Contains(j.Id))
                    .ToListAsync();

                var deletedCount = 0;
                foreach (var job in jobs)
                {
                    // Check permissions
                    if (job.CreatedByUserId != userId && userRole != "Admin")
                    {
                        continue;
                    }

                    // Cannot delete running jobs
                    if (job.Status == AppConst.JobStatus.RUNNING)
                    {
                        continue;
                    }

                    _context.AppJobs.Remove(job);
                    deletedCount++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk deleted {Count} jobs out of {Total} requested", deletedCount, jobIds.Count);
                return deletedCount > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error bulk deleting jobs");
                return false;
            }
        }

        public async Task<List<AppJob>> GetPendingJobsAsync()
        {
            return await _context.AppJobs
                .Include(j => j.Message)
                .Include(j => j.Instance)
                .Where(j => j.Status == AppConst.JobStatus.PENDING && (j.ScheduledAt == null || j.ScheduledAt <= DateTime.Now))
                .OrderBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task LogJobMessageAsync(int jobId, string level, string message, string? details = null)
        {
            try
            {
                var log = new AppJobLog
                {
                    JobId = jobId,
                    LogLevel = level,
                    Message = message,
                    Details = details
                };

                _context.AppJobLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging job message");
            }
        }
    }
}
