using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.Models
{
    public class AppInstance
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string WhatsAppNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string WhapiToken { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? WhapiUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<AppUserInstance> UserInstances { get; set; } = new List<AppUserInstance>();
        public virtual ICollection<AppJob> Jobs { get; set; } = new List<AppJob>();
        public virtual ICollection<AppGroup> Groups { get; set; } = new List<AppGroup>();
    }
}
