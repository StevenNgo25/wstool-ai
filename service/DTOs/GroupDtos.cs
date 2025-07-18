using WhatsAppCampaignManager.Models;

namespace WhatsAppCampaignManager.DTOs
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ParticipantCount { get; set; }
        public bool IsActive { get; set; }
        public int InstanceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public AppInstance Instance { get; set; } = null!;
    }

    public class GroupSearchRequest : PaginationRequest
    {
        public List<int>? InstanceIds { get; set; }
    }

    //public class DashboardStatsDto
    //{
    //    public int TotalGroups { get; set; }
    //    public int TotalUniqueUsers { get; set; }
    //    public int TotalMessages { get; set; }
    //    public int TotalJobs { get; set; }
    //    public int PendingJobs { get; set; }
    //    public int CompletedJobs { get; set; }
    //    public int TotalSentMessages { get; set; }
    //    public List<GroupStatsDto> TopGroups { get; set; } = new List<GroupStatsDto>();
    //}

    //public class GroupStatsDto
    //{
    //    public string GroupName { get; set; } = string.Empty;
    //    public int ParticipantCount { get; set; }
    //    public int MessagesSent { get; set; }
    //}
}
