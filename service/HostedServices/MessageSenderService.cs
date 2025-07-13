using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WhatsAppCampaignManager.Constants;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.Models;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.HostedServices
{
    public class MessageSenderService : BaseJobProcessorService
    {
        public MessageSenderService(IServiceScopeFactory scopeFactory, ILogger<MessageSenderService> logger)
            : base(scopeFactory, logger, maxConcurrencyPerUser: 2, processingIntervalMinutes: 1)
        {
        }

        protected override async Task ProcessSingleJobAsync(AppJob job, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Processing job {JobId}: {JobName} for user {UserId}", 
                job.Id, job.Name, job.CreatedByUserId);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();
            var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

            try
            {
                // Load job with related data
                var fullJob = await context.AppJobs
                    .Include(j => j.Message)
                    .Include(j => j.Instance)
                    .Include(j => j.CreatedByUser)
                    .FirstOrDefaultAsync(j => j.Id == job.Id, stoppingToken);

                if (fullJob == null)
                {
                    _logger.LogWarning("Job {JobId} not found", job.Id);
                    return;
                }

                await LogJobMessage(fullJob.Id, "Info", "Job started processing", context, stoppingToken);

                if (fullJob.JobType == "SendToGroups")
                {
                    await ProcessGroupJob(fullJob, context, whapiService, fileService, stoppingToken);
                }
                else if (fullJob.JobType == "SendToUsers")
                {
                    await ProcessUserJob(fullJob, context, whapiService, fileService, stoppingToken);
                }

                await MarkJobAsCompleted(fullJob.Id, stoppingToken);
                await LogJobMessage(fullJob.Id, "Info", "Job completed successfully", context, stoppingToken);
                
                _logger.LogInformation("Successfully completed job {JobId} for user {UserId}", 
                    fullJob.Id, fullJob.CreatedByUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job {JobId} for user {UserId}", 
                    job.Id, job.CreatedByUserId);
                
                using var errorScope = _scopeFactory.CreateScope();
                var errorContext = errorScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await LogJobMessage(job.Id, "Error", "Job failed", errorContext, stoppingToken, ex.Message);
                
                throw; // Re-throw to be handled by base class
            }
        }

        private async Task ProcessGroupJob(AppJob job, ApplicationDbContext context, IWhapiService whapiService, IFileService fileService, CancellationToken stoppingToken)
        {
            List<AppGroup> groups;

            if (string.IsNullOrEmpty(job.TargetData))
            {
                // Send to all assigned groups
                groups = await context.AppMessageGroups
                    .Include(mg => mg.Group)
                    .Where(mg => mg.MessageId == job.MessageId)
                    .Select(mg => mg.Group)
                    .ToListAsync(stoppingToken);
            }
            else
            {
                // Send to specific groups
                var groupIds = JsonSerializer.Deserialize<List<int>>(job.TargetData);
                if (groupIds == null || !groupIds.Any())
                {
                    await LogJobMessage(job.Id, "Warning", "No target groups specified", context, stoppingToken);
                    return;
                }

                groups = await context.AppGroups
                    .Where(g => groupIds.Contains(g.Id))
                    .ToListAsync(stoppingToken);
            }

            if (!groups.Any())
            {
                await LogJobMessage(job.Id, "Warning", "No groups found to send messages", context, stoppingToken);
                return;
            }

            _logger.LogInformation("Sending message to {GroupCount} groups for job {JobId}", groups.Count, job.Id);

            // Convert image to base64 if exists
            string? imageBase64 = null;
            if (!string.IsNullOrEmpty(job.Message.ImageUrl))
            {
                imageBase64 = await fileService.ConvertFileToBase64Async(job.Message.ImageUrl);
                if (imageBase64 == null)
                {
                    await LogJobMessage(job.Id, "Warning", $"Failed to convert image to base64: {job.Message.ImageUrl}", context, stoppingToken);
                }
            }

            foreach (var group in groups)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await SendMessageToGroup(job, group, context, whapiService, imageBase64, stoppingToken);
                
                // Small delay between messages to avoid rate limiting
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessUserJob(AppJob job, ApplicationDbContext context, IWhapiService whapiService, IFileService fileService, CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(job.TargetData))
            {
                await LogJobMessage(job.Id, "Warning", "No target users specified", context, stoppingToken);
                return;
            }

            var phoneNumbers = JsonSerializer.Deserialize<List<string>>(job.TargetData);
            if (phoneNumbers == null || !phoneNumbers.Any())
            {
                await LogJobMessage(job.Id, "Warning", "No valid phone numbers found", context, stoppingToken);
                return;
            }

            _logger.LogInformation("Sending message to {UserCount} users for job {JobId}", phoneNumbers.Count, job.Id);

            // Convert image to base64 if exists
            string? imageBase64 = null;
            if (!string.IsNullOrEmpty(job.Message.ImageUrl))
            {
                imageBase64 = await fileService.ConvertFileToBase64Async(job.Message.ImageUrl);
                if (imageBase64 == null)
                {
                    await LogJobMessage(job.Id, "Warning", $"Failed to convert image to base64: {job.Message.ImageUrl}", context, stoppingToken);
                }
            }

            foreach (var phoneNumber in phoneNumbers)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await SendMessageToUser(job, phoneNumber, context, whapiService, imageBase64, stoppingToken);
                
                // Small delay between messages to avoid rate limiting
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task SendMessageToGroup(AppJob job, AppGroup group, ApplicationDbContext context, IWhapiService whapiService, string? imageBase64, CancellationToken stoppingToken)
        {
            try
            {
                var message = job.Message.TextContent ?? "";

                var response = await whapiService.SendMessageAsync(
                    job.Instance.WhapiToken, 
                    group.GroupId, 
                    message, 
                    imageBase64, 
                    job.Instance.WhapiUrl);

                var sentMessage = new AppSentMessage
                {
                    JobId = job.Id,
                    RecipientId = group.GroupId,
                    RecipientType = "Group",
                    WhapiMessageId = response.Id,
                    Status = response.Sent ? "Sent" : "Failed",
                    ErrorMessage = response.Sent ? null : response.Error,
                    SentAt = DateTime.UtcNow
                };

                context.AppSentMessages.Add(sentMessage);
                await context.SaveChangesAsync(stoppingToken);

                await LogJobMessage(job.Id, response.Sent ? "Info" : "Warning", 
                    $"Message {(response.Sent ? "sent to" : "failed to send to")} group: {group.Name}", 
                    context, stoppingToken);

                _logger.LogDebug("Message {Status} to group {GroupName} for job {JobId}", 
                    response.Sent ? "sent" : "failed", group.Name, job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to group {GroupName} for job {JobId}", group.Name, job.Id);
                await LogJobMessage(job.Id, "Error", $"Failed to send message to group: {group.Name}", 
                    context, stoppingToken, ex.Message);
            }
        }

        private async Task SendMessageToUser(AppJob job, string phoneNumber, ApplicationDbContext context, IWhapiService whapiService, string? imageBase64, CancellationToken stoppingToken)
        {
            try
            {
                var message = job.Message.TextContent ?? "";

                var response = await whapiService.SendMessageAsync(
                    job.Instance.WhapiToken, 
                    phoneNumber, 
                    message, 
                    imageBase64, 
                    job.Instance.WhapiUrl);

                var sentMessage = new AppSentMessage
                {
                    JobId = job.Id,
                    RecipientId = phoneNumber,
                    RecipientType = "User",
                    WhapiMessageId = response.Id,
                    Status = response.Sent ? "Sent" : "Failed",
                    ErrorMessage = response.Sent ? null : response.Error,
                    SentAt = DateTime.UtcNow
                };

                context.AppSentMessages.Add(sentMessage);
                await context.SaveChangesAsync(stoppingToken);

                await LogJobMessage(job.Id, response.Sent ? "Info" : "Warning", 
                    $"Message {(response.Sent ? "sent to" : "failed to send to")} user: {phoneNumber}", 
                    context, stoppingToken);

                _logger.LogDebug("Message {Status} to user {PhoneNumber} for job {JobId}", 
                    response.Sent ? "sent" : "failed", phoneNumber, job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to user {PhoneNumber} for job {JobId}", phoneNumber, job.Id);
                await LogJobMessage(job.Id, "Error", $"Failed to send message to user: {phoneNumber}", 
                    context, stoppingToken, ex.Message);
            }
        }

        private async Task LogJobMessage(int jobId, string level, string message, ApplicationDbContext context, CancellationToken stoppingToken, string? details = null)
        {
            try
            {
                var log = new AppJobLog
                {
                    JobId = jobId,
                    LogLevel = level,
                    Message = message,
                    Details = details,
                    CreatedAt = DateTime.UtcNow
                };

                context.AppJobLogs.Add(log);
                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging job message for job {JobId}", jobId);
            }
        }
    }
}
