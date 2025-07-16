using WhatsAppCampaignManager.DTOs;

namespace WhatsAppCampaignManager.Services
{
    public interface IGroupService
    {
        Task<PaginatedResponse<GroupDto>> GetGroupsAsync(PaginationRequest request, int userId, string userRole);
        Task<PaginatedResponse<GroupDto>> SearchGroupsByInstancesAsync(GroupSearchRequest request, int userId, string userRole);
        Task<GroupDto?> GetGroupByIdAsync(int id);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
