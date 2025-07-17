using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.DTOs
{
    public class CreateInstanceDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string WhatsAppNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string WhapiToken { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? WhapiUrl { get; set; }
    }

    public class UpdateInstanceDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(200)]
        public string? WhapiToken { get; set; }
        
        [StringLength(200)]
        public string? WhapiUrl { get; set; }
        
        public bool? IsActive { get; set; }

        public string? WhatsAppNumber { get; set; }
    }

    public class InstanceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? WhatsAppNumber { get; set; }
        public string WhapiToken { get; set; } = string.Empty;
        public string? WhapiUrl { get; set; }
        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AssignUserDto
    {
        public int UserId { get; set; }
        public bool CanSendMessages { get; set; } = true;
        public bool CanCreateJobs { get; set; } = true;
    }
}
