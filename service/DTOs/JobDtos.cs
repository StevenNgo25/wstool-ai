using System.ComponentModel.DataAnnotations;
using WhatsAppCampaignManager.Models;

namespace WhatsAppCampaignManager.DTOs
{
    public class CreateJobDto
    {
        
        [Required]
        [StringLength(20)]
        public string JobType { get; set; } = "SendToGroups";
        
        [Required]
        public int MessageId { get; set; }
        
        [Required]
        public int InstanceId { get; set; }
        
        public DateTime? ScheduledAt { get; set; }
        
        public List<int>? GroupIds { get; set; }
        public List<string>? PhoneNumbers { get; set; }
    }

    public class JobDto
    {
        public int Id { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MessageTitle { get; set; } = string.Empty;
        public string InstanceName { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalSentMessages { get; set; }
        public int SuccessfulMessages { get; set; }
        public int FailedMessages { get; set; }
        public List<JobLogDto> Logs { get; set; } = new List<JobLogDto>();
        public List<JobSentMessageDto> SentMessages { get; set; } = new List<JobSentMessageDto>();
        public List<GroupDto> AssignedGroups { get; set; } = new List<GroupDto>();
        public List<string>? TargetPhoneNumbers { get; set; }
    }

    public class JobLogDto
    {
        public int Id { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class JobSentMessageDto
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public string RecipientId { get; set; } = string.Empty; // Group ID or Phone Number

        public string RecipientType { get; set; } = "Group"; // Group, User

        public string? WhapiMessageId { get; set; }

        public string Status { get; set; } = "Sent"; // Sent, Delivered, Read, Failed

        public string? ErrorMessage { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public DateTime? DeliveredAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime? LastValidatedAt { get; set; }
    }

    public class BulkDeleteJobsRequest
    {
        [Required]
        public List<int> JobIds { get; set; } = new List<int>();
    }
}
