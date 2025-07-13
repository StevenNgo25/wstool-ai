using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.Models
{
    public class AppSentMessage
    {
        public int Id { get; set; }
        
        public int JobId { get; set; }
        public virtual AppJob Job { get; set; } = null!;
        
        [StringLength(100)]
        public string RecipientId { get; set; } = string.Empty; // Group ID or Phone Number
        
        [StringLength(20)]
        public string RecipientType { get; set; } = "Group"; // Group, User
        
        [StringLength(100)]
        public string? WhapiMessageId { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Sent"; // Sent, Delivered, Read, Failed
        
        [StringLength(500)]
        public string? ErrorMessage { get; set; }
        
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? DeliveredAt { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        public DateTime? LastValidatedAt { get; set; }
    }
}
