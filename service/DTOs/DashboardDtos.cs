namespace WhatsAppCampaignManager.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalInstances { get; set; }
        public int TotalMessages { get; set; }
        public int TotalGroups { get; set; }
        public int TotalJobs { get; set; }
        public int PendingJobs { get; set; }
        public int RunningJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int ActiveInstances { get; set; }
        public int TotalSentMessages { get; set; }
        public int SuccessfulMessages { get; set; }
        public int FailedMessages { get; set; }
        public int Last7DaysSentMessages { get; set; }
        public int Last7DaysFailedMessages { get; set; }
        public int RecentJobs { get; set; }
        public int RecentMessages { get; set; }

        public int TotalUniqueUsers { get; set; }
        public List<GroupStatsDto> TopGroups { get; set; } = new List<GroupStatsDto>();
    }

    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Job", "Message", "Instance", etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class JobStatusStatsDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class InstanceStatsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalGroups { get; set; }
        public int ActiveGroups { get; set; }
        public int TotalMessages { get; set; }
        public int TotalJobs { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class GroupStatsDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public int MessagesSent { get; set; }
    }
}
