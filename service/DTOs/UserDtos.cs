using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<InstanceDto> AssignedInstances { get; set; } = new List<InstanceDto>();
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = "Member"; // Admin, Member
    }

    public class UpdateUserDto
    {
        [StringLength(100)]
        public string? Username { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [MinLength(6)]
        public string? Password { get; set; }
        
        public string? Role { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class AssignInstancesDto
    {
        [Required]
        public List<int> InstanceIds { get; set; } = new List<int>();
        public bool CanSendMessages { get; set; } = true;
        public bool CanCreateJobs { get; set; } = true;
    }
}
