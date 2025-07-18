using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Extensions;

namespace WhatsAppCampaignManager.Services.Implements
{
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GroupService> _logger;

        public GroupService(ApplicationDbContext context, ILogger<GroupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResponse<GroupDto>> GetGroupsAsync(PaginationRequest request, int userId, string userRole)
        {
            var query = _context.AppGroups
                .Where(g => g.IsActive)
                .Include(i => i.Instance)
                .Where(q => userRole == "Admin" || q.Instance.UserInstances.Any(q => q.UserId == userId))
                .Where(q => q.Instance.IsActive)
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    GroupId = g.GroupId,
                    Name = g.Name,
                    Description = g.Description,
                    ParticipantCount = g.ParticipantCount,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt,
                    LastSyncAt = g.LastSyncAt,
                    Instance = g.Instance
                });

            // Apply search
            query = query.ApplySearch(request.Search,
                x => x.Name,
                x => x.Description ?? "",
                x => x.GroupId);

            // Apply sorting
            query = query.ApplySort(request.SortBy, request.SortDirection);

            // If no sort specified, default to Name asc
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.OrderBy(x => x.Name);
            }

            return await query.ToPaginatedResponseAsync(request);
        }

        public async Task<PaginatedResponse<GroupDto>> SearchGroupsByInstancesAsync(GroupSearchRequest request, int userId, string userRole)
        {
            var query = _context.AppGroups
                .Where(g => g.IsActive)
                .Include(i => i.Instance)
                .Where(q => userRole == "Admin" || q.Instance.UserInstances.Any(q => q.UserId == userId))
                .Where(q => q.Instance.IsActive)
                .AsQueryable();

            // Filter by instance IDs if provided
            if (request.InstanceIds != null && request.InstanceIds.Any())
            {
                // Get groups that are associated with messages from specified instances
                //var groupIdsFromInstances = await _context.AppMessages
                //    .Where(m => request.InstanceIds.Contains(m.InstanceId))
                //    .SelectMany(m => m.MessageGroups)
                //    .Select(mg => mg.GroupId)
                //    .Distinct()
                //    .ToListAsync();

                query = query.Where(g => request.InstanceIds.Contains(g.InstanceId));
            }

            var groupQuery = query.Select(g => new GroupDto
            {
                Id = g.Id,
                GroupId = g.GroupId,
                Name = g.Name,
                Description = g.Description,
                ParticipantCount = g.ParticipantCount,
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                LastSyncAt = g.LastSyncAt,
                InstanceId = g.InstanceId
            });

            // Apply search
            groupQuery = groupQuery.ApplySearch(request.Search,
                x => x.Name,
                x => x.Description ?? "",
                x => x.GroupId);

            // Apply sorting
            groupQuery = groupQuery.ApplySort(request.SortBy, request.SortDirection);

            // If no sort specified, default to Name asc
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                groupQuery = groupQuery.OrderBy(x => x.Name);
            }

            return await groupQuery.ToPaginatedResponseAsync(request);
        }

        public async Task<GroupDto?> GetGroupByIdAsync(int id)
        {
            return await _context.AppGroups
                .Where(g => g.Id == id)
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    GroupId = g.GroupId,
                    Name = g.Name,
                    Description = g.Description,
                    ParticipantCount = g.ParticipantCount,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt,
                    LastSyncAt = g.LastSyncAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(int userId, string role)
        {
            try
            {
                var userInstance = await _context.AppUserInstances.AsNoTracking().Where(q => q.UserId == userId).Select(s => s.InstanceId).ToListAsync();

                var stats = new DashboardStatsDto
                {
                    TotalGroups = await _context.AppGroups.Include(i => i.Instance).Where(q => role == "Admin" || userInstance.Contains(q.InstanceId)).CountAsync(g => g.IsActive),
                    TotalUniqueUsers = await _context.AppAnalytics.Include(i => i.Group).Where(q => role == "Admin" || userInstance.Contains(q.Group.InstanceId)).Select(a => a.UserPhoneNumber).Distinct().CountAsync(),
                    TotalMessages = await _context.AppMessages.Where(q => q.CreatedByUserId == userId).CountAsync(),
                    TotalJobs = await _context.AppJobs.Where(q => q.CreatedByUserId == userId).CountAsync(),
                    PendingJobs = await _context.AppJobs.Where(q => q.CreatedByUserId == userId).CountAsync(j => j.Status == "Pending"),
                    RunningJobs = await _context.AppJobs.Where(q => q.CreatedByUserId == userId).CountAsync(j => j.Status == "Running"),
                    CompletedJobs = await _context.AppJobs.Where(q => q.CreatedByUserId == userId).CountAsync(j => j.Status == "Completed"),
                    TotalSentMessages = await _context.AppSentMessages.Include(i => i.Job).Where(q=>q.Job.CreatedByUserId == userId).CountAsync(),
                    TopGroups = await _context.AppGroups
                    .Include(i => i.Instance).Where(q => role == "Admin" || userInstance.Contains(q.InstanceId))
                        .Where(g => g.IsActive)
                        .OrderByDescending(g => g.ParticipantCount)
                        .Take(5)
                        .Select(g => new GroupStatsDto
                        {
                            GroupName = g.Name,
                            ParticipantCount = g.ParticipantCount,
                            MessagesSent = _context.AppSentMessages.Count(sm => sm.RecipientId == g.GroupId)
                        })
                        .ToListAsync()
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return new DashboardStatsDto();
            }
        }
    }
}
