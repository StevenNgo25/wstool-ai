using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.Models
{
    public class AppJobLog
    {
        public int Id { get; set; }
        
        public int JobId { get; set; }
        public virtual AppJob Job { get; set; } = null!;
        
        [StringLength(20)]
        public string LogLevel { get; set; } = "Info"; // Info, Warning, Error
        
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(4000)]
        public string? Details { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
