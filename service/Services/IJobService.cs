using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Models;

namespace WhatsAppCampaignManager.Services
{
    public interface IJobService
    {
        Task<JobDto?> CreateJobAsync(CreateJobDto createJobDto, int userId);
        Task<PaginatedResponse<JobDto>> GetJobsAsync(PaginationRequest request, int? userId = null);
        Task<JobDto?> GetJobByIdAsync(int id);
        Task<bool> UpdateJobStatusAsync(int jobId, string status);
        Task<bool> RestartJobAsync(int jobId, int userId, string userRole);
        Task<bool> DeleteJobAsync(int id, int userId);
        Task<bool> BulkDeleteJobsAsync(List<int> jobIds, int userId, string userRole);
        Task<List<AppJob>> GetPendingJobsAsync();
        Task LogJobMessageAsync(int jobId, string level, string message, string? details = null);
    }
}
