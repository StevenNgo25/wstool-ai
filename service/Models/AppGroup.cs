using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.Models
{
    public class AppGroup
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string GroupId { get; set; } = string.Empty; // WhatsApp Group ID
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int ParticipantCount { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastSyncAt { get; set; }

        public int InstanceId { get; set; }
        public virtual AppInstance Instance { get; set; } = null!;

        // Navigation properties

        public virtual ICollection<AppMessageGroup> MessageGroups { get; set; } = new List<AppMessageGroup>();
        public virtual ICollection<AppAnalys> Analytics { get; set; } = new List<AppAnalys>();
    }
}
