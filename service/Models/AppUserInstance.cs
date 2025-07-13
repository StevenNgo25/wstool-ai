namespace WhatsAppCampaignManager.Models
{
    public class AppUserInstance
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public virtual AppUser User { get; set; } = null!;
        
        public int InstanceId { get; set; }
        public virtual AppInstance Instance { get; set; } = null!;
        
        public bool CanSendMessages { get; set; } = true;
        public bool CanCreateJobs { get; set; } = true;
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
