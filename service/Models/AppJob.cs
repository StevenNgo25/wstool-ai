using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.Models
{
    public class AppJob
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(20)]
        public string JobType { get; set; } = "SendToGroups"; // SendToGroups, SendToUsers
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed
        
        public int MessageId { get; set; }
        public virtual AppMessage Message { get; set; } = null!;
        
        public int InstanceId { get; set; }
        public virtual AppInstance Instance { get; set; } = null!;
        
        public int CreatedByUserId { get; set; }
        public virtual AppUser CreatedByUser { get; set; } = null!;
        
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // JSON serialized list of group IDs or user phone numbers
        [StringLength(4000)]
        public string? TargetData { get; set; }
        
        // Navigation properties
        public virtual ICollection<AppJobLog> JobLogs { get; set; } = new List<AppJobLog>();
        public virtual ICollection<AppSentMessage> SentMessages { get; set; } = new List<AppSentMessage>();
    }
}
