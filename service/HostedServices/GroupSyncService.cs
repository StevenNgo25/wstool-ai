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

        //private async Task SyncGroupsFromAllInstances(CancellationToken stoppingToken)
        //{
        //    using var scope = _serviceProvider.CreateScope();
        //    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //    var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();

        //    var activeInstances = await context.AppInstances
        //        .Where(i => i.IsActive)
        //        .ToListAsync(stoppingToken);

        //    const int pageSize = 50;

        //    foreach (var instance in activeInstances)
        //    {
        //        if (stoppingToken.IsCancellationRequested)
        //            break;

        //        try
        //        {
        //            _logger.LogInformation("Syncing groups for instance {InstanceName}", instance.Name);

        //            var fetchedIds = new List<string>();
        //            int page = 0;
        //            bool hasMore = true;

        //            while (hasMore && !stoppingToken.IsCancellationRequested)
        //            {
        //                var groups = await whapiService.GetGroupsAsync(
        //                    instance.WhapiToken,
        //                    instance.WhapiUrl,
        //                    pageSize,
        //                    offset: page * pageSize);

        //                if (groups.items == null || groups.items.Count == 0)
        //                    break;

        //                foreach (var whapiGroup in groups.items.Where(q => q.ParticipantCount >= 20))
        //                {
        //                    fetchedIds.Add(whapiGroup.Id);

        //                    var existingGroup = await context.AppGroups
        //                        .FirstOrDefaultAsync(g => g.GroupId == whapiGroup.Id && g.InstanceId == instance.Id, stoppingToken);

        //                    if (existingGroup == null)
        //                    {
        //                        var newGroup = new AppGroup
        //                        {
        //                            GroupId = whapiGroup.Id,
        //                            Name = whapiGroup.Name,
        //                            Description = whapiGroup.Description ?? whapiGroup.Subject,
        //                            ParticipantCount = whapiGroup.ParticipantCount,
        //                            Participants = whapiGroup.Participants != null ? string.Join(';', whapiGroup.Participants) : null,
        //                            LastSyncAt = DateTime.Now,
        //                            InstanceId = instance.Id,
        //                        };

        //                        context.AppGroups.Add(newGroup);
        //                        _logger.LogInformation("Added new group: {GroupName}", whapiGroup.Name);
        //                    }
        //                    else
        //                    {
        //                        existingGroup.Name = whapiGroup.Name;
        //                        existingGroup.Description = whapiGroup.Description ?? whapiGroup.Subject;
        //                        existingGroup.ParticipantCount = whapiGroup.ParticipantCount;
        //                        existingGroup.Participants = whapiGroup.Participants != null ? string.Join(';', whapiGroup.Participants) : null;
        //                        existingGroup.LastSyncAt = DateTime.Now;
        //                        existingGroup.IsActive = true;
        //                        existingGroup.InstanceId = instance.Id;
        //                    }
        //                }

        //                // Nếu số lượng nhóm trả về < pageSize thì không còn trang tiếp theo
        //                hasMore = groups.items.Count == pageSize;
        //                page++;

        //                await context.SaveChangesAsync(stoppingToken);
        //            }

        //            // Xóa những nhóm không còn trong API nữa
        //            if (fetchedIds.Count > 0)
        //            {
        //                var toDelete = await context.AppGroups
        //                    .Where(x => !fetchedIds.Contains(x.GroupId) && x.InstanceId == instance.Id)
        //                    .ToListAsync(stoppingToken);

        //                if (toDelete.Any())
        //                {
        //                    context.AppGroups.RemoveRange(toDelete);
        //                }
        //            }

        //            await context.SaveChangesAsync(stoppingToken);

        //            _logger.LogInformation("Successfully synced {GroupCount} groups for instance {InstanceName}",
        //                fetchedIds.Count, instance.Name);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error syncing groups for instance {InstanceName}", instance.Name);
        //        }
        //    }
        //}

        private async Task SyncGroupsFromAllInstances(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();

            var activeInstances = await context.AppInstances
                .Where(i => i.IsActive && i.Id == 3)
                .ToListAsync(stoppingToken);

            const int maxConcurrency = 5;
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            foreach (var instance in activeInstances)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await semaphore.WaitAsync(stoppingToken);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await SyncGroupsForInstance(instance, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing groups for instance {InstanceName}", instance.Name);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, stoppingToken));
            }

            await Task.WhenAll(tasks);
        }


        private async Task SyncGroupsForInstance(AppInstance instance, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();

            _logger.LogInformation("Syncing groups for instance {InstanceName}", instance.Name);

            var fetchedIds = new List<string>();
            int page = 0;
            const int pageSize = 20;
            bool hasMore = true;

            while (hasMore && !stoppingToken.IsCancellationRequested)
            {
                var groups = await whapiService.GetGroupsAsync(
                    instance.WhapiToken,
                    instance.WhapiUrl,
                    pageSize,
                    offset: page * pageSize);

                if (groups.items == null || groups.items.Count == 0)
                    break;

                foreach (var whapiGroup in groups.items.Where(q => q.ParticipantCount >= 20))
                {
                    fetchedIds.Add(whapiGroup.Id);

                    var existingGroup = await context.AppGroups
                        .FirstOrDefaultAsync(g => g.GroupId == whapiGroup.Id && g.InstanceId == instance.Id, stoppingToken);

                    if (existingGroup == null)
                    {
                        var newGroup = new AppGroup
                        {
                            GroupId = whapiGroup.Id,
                            Name = whapiGroup.Name,
                            Description = whapiGroup.Description ?? whapiGroup.Subject,
                            ParticipantCount = whapiGroup.ParticipantCount,
                            Participants = whapiGroup.Participants != null ? string.Join(';', whapiGroup.Participants) : null,
                            LastSyncAt = DateTime.Now,
                            InstanceId = instance.Id,
                        };

                        context.AppGroups.Add(newGroup);
                        _logger.LogInformation("Added new group: {GroupName}", whapiGroup.Name);
                    }
                    else
                    {
                        existingGroup.Name = whapiGroup.Name;
                        existingGroup.Description = whapiGroup.Description ?? whapiGroup.Subject;
                        existingGroup.ParticipantCount = whapiGroup.ParticipantCount;
                        existingGroup.Participants = whapiGroup.Participants != null ? string.Join(';', whapiGroup.Participants) : null;
                        existingGroup.LastSyncAt = DateTime.Now;
                        existingGroup.IsActive = true;
                        existingGroup.InstanceId = instance.Id;
                    }
                }

                hasMore = groups.items.Count == pageSize;
                page++;

                await context.SaveChangesAsync(stoppingToken);
            }

            // Xóa những nhóm không còn trong API nữa
            if (fetchedIds.Count > 0)
            {
                var toDelete = await context.AppGroups
                    .Where(x => !fetchedIds.Contains(x.GroupId) && x.InstanceId == instance.Id)
                    .ToListAsync(stoppingToken);

                if (toDelete.Any())
                {
                    context.AppGroups.RemoveRange(toDelete);
                }

                await context.SaveChangesAsync(stoppingToken);
            }

            _logger.LogInformation("Successfully synced {GroupCount} groups for instance {InstanceName}",
                fetchedIds.Count, instance.Name);
        }


    }
}
