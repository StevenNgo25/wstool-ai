using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.DTOs
{
    public class CreateMessageDto
    {
        [Required]
        [StringLength(4000)]
        public string TextContent { get; set; } = string.Empty;
        
        public int? InstanceId { get; set; }
        
        public List<int> GroupIds { get; set; } = new List<int>();
    }

    public class CreateMessageWithFileDto
    {
        [Required]
        [StringLength(4000)]
        public string TextContent { get; set; } = string.Empty;

        public int? InstanceId { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        public List<int> GroupIds { get; set; } = new List<int>();
    }

    public class UpdateMessageDto
    {
        
        [StringLength(4000)]
        public string? TextContent { get; set; }
        
        public int? InstanceId { get; set; }
        
        public List<int>? GroupIds { get; set; }
    }

    public class UpdateMessageWithFileDto
    {
        
        [StringLength(4000)]
        public string? TextContent { get; set; }
        
        public int? InstanceId { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        public bool RemoveImage { get; set; } = false;
        
        public List<int>? GroupIds { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string? TextContent { get; set; }
        public string? ImageUrl { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public int? InstanceId { get; set; }
        public string InstanceName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<GroupDto> AssignedGroups { get; set; } = new List<GroupDto>();
    }

    public class BulkDeleteRequest
    {
        [Required]
        public List<int> Ids { get; set; } = new List<int>();
    }
}
