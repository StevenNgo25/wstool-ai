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
                .Include(i=>i.Instance)
                .Where(q=> userRole == "Admin" || q.Instance.UserInstances.Any(q=>q.UserId == userId))
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
                    Instance= g.Instance
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
                LastSyncAt = g.LastSyncAt
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

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var stats = new DashboardStatsDto
                {
                    TotalGroups = await _context.AppGroups.CountAsync(g => g.IsActive),
                    TotalUniqueUsers = await _context.AppAnalytics.Select(a => a.UserPhoneNumber).Distinct().CountAsync(),
                    TotalMessages = await _context.AppMessages.CountAsync(),
                    TotalJobs = await _context.AppJobs.CountAsync(),
                    PendingJobs = await _context.AppJobs.CountAsync(j => j.Status == "Pending"),
                    CompletedJobs = await _context.AppJobs.CountAsync(j => j.Status == "Completed"),
                    TotalSentMessages = await _context.AppSentMessages.CountAsync(),
                    TopGroups = await _context.AppGroups
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
