namespace WhatsAppCampaignManager.Models
{
    public class AppMessageGroup
    {
        public int Id { get; set; }
        
        public int MessageId { get; set; }
        public virtual AppMessage Message { get; set; } = null!;
        
        public int GroupId { get; set; }
        public virtual AppGroup Group { get; set; } = null!;
        
        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}
