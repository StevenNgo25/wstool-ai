using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.Models;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.HostedServices
{
    public class GroupSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GroupSyncService> _logger;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(30); // Sync every 30 minutes

        public GroupSyncService(IServiceProvider serviceProvider, ILogger<GroupSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Group Sync Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncGroupsFromAllInstances(stoppingToken);
                    await Task.Delay(_syncInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Group Sync Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Group Sync Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
                }
            }
        }

        private async Task SyncGroupsFromAllInstances(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();

            var activeInstances = await context.AppInstances
                .Where(i => i.IsActive)
                .ToListAsync(stoppingToken);

            foreach (var instance in activeInstances)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation("Syncing groups for instance {InstanceName}", instance.Name);

                    var groups = await whapiService.GetGroupsAsync(instance.WhapiToken, instance.WhapiUrl,300);
                    var fetchedIds = new List<string>();
                    foreach (var whapiGroup in groups.items.Where(q=>q.ParticipantCount >= 20))
                    {
                        fetchedIds.Add(whapiGroup.Id);
                        var existingGroup = await context.AppGroups
                            .FirstOrDefaultAsync(g => g.GroupId == whapiGroup.Id && g.InstanceId == instance.Id, stoppingToken);

                        if (existingGroup == null)
                        {
                            // Create new group
                            var newGroup = new AppGroup
                            {
                                GroupId = whapiGroup.Id,
                                Name = whapiGroup.Name,
                                Description = whapiGroup.Description ?? whapiGroup.Subject,
                                ParticipantCount = whapiGroup.ParticipantCount,
                                LastSyncAt = DateTime.UtcNow,
                                InstanceId = instance.Id,
                            };

                            context.AppGroups.Add(newGroup);
                            _logger.LogInformation("Added new group: {GroupName}", whapiGroup.Name);
                        }
                        else
                        {
                            // Update existing group
                            existingGroup.Name = whapiGroup.Name;
                            existingGroup.Description = whapiGroup.Description ?? whapiGroup.Subject;
                            existingGroup.ParticipantCount = whapiGroup.ParticipantCount;
                            existingGroup.LastSyncAt = DateTime.UtcNow;
                            existingGroup.IsActive = true;
                            existingGroup.InstanceId = instance.Id;
                        }
                    }

                    // Xóa những chat không còn trong API nữa
                    if (fetchedIds.Count > 0)
                    {
                        var toDelete = await context.AppGroups
                                               .Where(x => !fetchedIds.Contains(x.GroupId) && x.InstanceId == instance.Id)
                                               .ToListAsync(stoppingToken);
                        if (toDelete.Any())
                        {
                            context.AppGroups.RemoveRange(toDelete);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Successfully synced {GroupCount} groups for instance {InstanceName}",
                        groups.total, instance.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing groups for instance {InstanceName}", instance.Name);
                }
            }
        }
    }
}
