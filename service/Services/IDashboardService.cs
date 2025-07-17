using WhatsAppCampaignManager.DTOs;

namespace WhatsAppCampaignManager.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<List<RecentActivityDto>> GetRecentActivityAsync(int limit = 10);
        Task<List<JobStatusStatsDto>> GetJobStatusStatsAsync();
        Task<List<InstanceStatsDto>> GetInstanceStatsAsync();
    }
}
