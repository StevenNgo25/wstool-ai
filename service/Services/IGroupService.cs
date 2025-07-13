using WhatsAppCampaignManager.DTOs;

namespace WhatsAppCampaignManager.Services
{
    public interface IGroupService
    {
        Task<PaginatedResponse<GroupDto>> GetGroupsAsync(PaginationRequest request);
        Task<PaginatedResponse<GroupDto>> SearchGroupsByInstancesAsync(GroupSearchRequest request);
        Task<GroupDto?> GetGroupByIdAsync(int id);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
