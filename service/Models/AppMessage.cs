using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.Models
{
    public class AppMessage
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(4000)]
        public string? TextContent { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [StringLength(20)]
        public string MessageType { get; set; } = "Text"; // Text, Image, Mixed
        
        public int CreatedByUserId { get; set; }
        public virtual AppUser CreatedByUser { get; set; } = null!;
        
        public int InstanceId { get; set; }
        public virtual AppInstance Instance { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<AppMessageGroup> MessageGroups { get; set; } = new List<AppMessageGroup>();
        public virtual ICollection<AppJob> Jobs { get; set; } = new List<AppJob>();
    }
}
