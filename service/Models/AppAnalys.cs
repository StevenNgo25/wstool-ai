using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.Models
{
    public class AppAnalys
    {
        public int Id { get; set; }
        
        public int GroupId { get; set; }
        public virtual AppGroup Group { get; set; } = null!;
        
        [StringLength(50)]
        public string UserPhoneNumber { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? UserName { get; set; }
        
        public bool IsAdmin { get; set; } = false;
        
        public DateTime JoinedAt { get; set; }
        
        public DateTime LastSeenAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}
