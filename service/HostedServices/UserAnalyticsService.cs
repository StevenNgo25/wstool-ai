using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.Models;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.HostedServices
{
    public class UserAnalyticsService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserAnalyticsService> _logger;
        private readonly TimeSpan _analysisInterval = TimeSpan.FromHours(2); // Analyze every 2 hours

        public UserAnalyticsService(IServiceProvider serviceProvider, ILogger<UserAnalyticsService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("User Analytics Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await AnalyzeUsersInGroups();
                    await Task.Delay(_analysisInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("User Analytics Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in User Analytics Service");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }

        private async Task AnalyzeUsersInGroups()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();

            var activeInstances = await context.AppInstances
                .Where(i => i.IsActive)
                .ToListAsync();

            var activeGroups = await context.AppGroups
                .Where(g => g.IsActive)
                .ToListAsync();

            foreach (var instance in activeInstances)
            {
                foreach (var group in activeGroups)
                {
                    try
                    {
                        _logger.LogInformation("Analyzing users in group {GroupName} for instance {InstanceName}", 
                            group.Name, instance.Name);

                        var participants = await whapiService.GetGroupMembersAsync(instance.WhapiToken, group.GroupId, instance.WhapiUrl);

                        foreach (var participant in participants)
                        {
                            var existingAnalytic = await context.AppAnalytics
                                .FirstOrDefaultAsync(a => a.GroupId == group.Id && a.UserPhoneNumber == participant.Id);

                            if (existingAnalytic == null)
                            {
                                // Create new analytics record
                                var newAnalytic = new AppAnalys
                                {
                                    GroupId = group.Id,
                                    UserPhoneNumber = participant.Id,
                                    UserName = participant.Name ?? participant.PushName,
                                    IsAdmin = participant.IsAdmin || participant.IsSuperAdmin,
                                    JoinedAt = DateTime.UtcNow, // WHAPI doesn't provide join date
                                    LastSeenAt = DateTime.UtcNow
                                };

                                context.AppAnalytics.Add(newAnalytic);
                            }
                            else
                            {
                                // Update existing record
                                existingAnalytic.UserName = participant.Name ?? participant.PushName;
                                existingAnalytic.IsAdmin = participant.IsAdmin || participant.IsSuperAdmin;
                                existingAnalytic.LastSeenAt = DateTime.UtcNow;
                                existingAnalytic.UpdatedAt = DateTime.UtcNow;
                            }
                        }

                        await context.SaveChangesAsync();
                        _logger.LogInformation("Successfully analyzed {ParticipantCount} participants in group {GroupName}", 
                            participants.Count, group.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error analyzing users in group {GroupName} for instance {InstanceName}", 
                            group.Name, instance.Name);
                    }
                }
            }
        }
    }
}
