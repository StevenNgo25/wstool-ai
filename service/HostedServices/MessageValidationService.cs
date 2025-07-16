using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.HostedServices
{
    public class MessageValidationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageValidationService> _logger;
        private readonly TimeSpan _validationInterval = TimeSpan.FromMinutes(15); // Validate every 15 minutes

        public MessageValidationService(IServiceProvider serviceProvider, ILogger<MessageValidationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Message Validation Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ValidateSentMessages();
                    await Task.Delay(_validationInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Message Validation Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Message Validation Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task ValidateSentMessages()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();

            // Get messages that need validation (sent in last 24 hours and not validated recently)
            var cutoffTime = DateTime.Now.AddHours(-24);
            var lastValidationCutoff = DateTime.Now.AddMinutes(-30);

            var messagesToValidate = await context.AppSentMessages
                .Include(sm => sm.Job)
                    .ThenInclude(j => j.Instance)
                .Where(sm => sm.SentAt >= cutoffTime && 
                           !string.IsNullOrEmpty(sm.WhapiMessageId) &&
                           sm.Status == "Sent" &&
                           (sm.LastValidatedAt == null || sm.LastValidatedAt <= lastValidationCutoff))
                .Take(100) // Limit to avoid overwhelming the API
                .ToListAsync();

            foreach (var sentMessage in messagesToValidate)
            {
                try
                {
                    var status = await whapiService.GetMessageStatusAsync(
                        sentMessage.Job.Instance.WhapiToken,
                        sentMessage.WhapiMessageId!,
                        sentMessage.Job.Instance.WhapiUrl);

                    // Update message status based on WHAPI response
                    // WHAPI ack values: 0=sent, 1=delivered, 2=read, 3=played
                    sentMessage.Status = status.Ack switch
                    {
                        "1" => "Delivered",
                        "2" => "Read",
                        "3" => "Read", // Played (for voice messages)
                        "0" => "Sent",
                        _ => sentMessage.Status
                    };

                    // Convert timestamp to DateTime if available
                    if (status.Timestamp > 0)
                    {
                        var messageTime = DateTimeOffset.FromUnixTimeSeconds(status.Timestamp).DateTime;
                        
                        if (sentMessage.Status == "Delivered" && sentMessage.DeliveredAt == null)
                            sentMessage.DeliveredAt = messageTime;
                        
                        if (sentMessage.Status == "Read" && sentMessage.ReadAt == null)
                            sentMessage.ReadAt = messageTime;
                    }

                    sentMessage.LastValidatedAt = DateTime.Now;

                    _logger.LogDebug("Updated message {MessageId} status to {Status}", 
                        sentMessage.WhapiMessageId, sentMessage.Status);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating message {MessageId}", sentMessage.WhapiMessageId);
                    sentMessage.LastValidatedAt = DateTime.Now; // Mark as validated to avoid retry loops
                }
            }

            if (messagesToValidate.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Validated {Count} sent messages", messagesToValidate.Count);
            }
        }
    }
}
