using System.Text.Json.Serialization;

namespace WhatsAppCampaignManager.Services
{
    public interface IWhapiService
    {
        Task<(List<WhapiGroup> items, int total)> GetGroupsAsync(string token, string? baseUrl = null, int count = 20, int offset = 0);
        Task<List<WhapiGroupMember>> GetGroupMembersAsync(string token, string groupId, string? baseUrl = null);
        Task<WhapiMessageResponse> SendMessageAsync(string token, string groupId, string message, string? imageBase64 = null, string? baseUrl = null);
        Task<WhapiMessageStatus> GetMessageStatusAsync(string token, string messageId, string? baseUrl = null);
        Task<bool> ValidateTokenAsync(string token, string? baseUrl = null);

        Task<string?> GetQrCodeAsync(string token, string? baseUrl = null);
        Task<string?> GetRawCodeAsync(string phone, string token, string? baseUrl = null);
        Task<bool> Logout(string token, string? baseUrl = null);
    }

    // WHAPI Response Models based on official documentation
    public class WhapiBaseResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
    }

    public class WhapiGroup
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public long CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public bool Announce { get; set; }
        public bool Restrict { get; set; }
        public string? InviteCode { get; set; }
        public int ParticipantCount { get; set; }
        public string? RawData { get; set; }
    }

    public class WhapiGroupParticipant
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? PushName { get; set; }
        public string? ShortName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
    }

    public class WhapiGroupMember
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? PushName { get; set; }
        public string? ShortName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
    }

    public class WhapiSendMessageResponse
    {
        public bool Sent { get; set; }
        public string? Id { get; set; }
        public string? Message { get; set; }
        public WhapiMessageDetails? Details { get; set; }
    }

    public class WhapiMessageResponse
    {
        public bool Sent { get; set; }
        public string? Id { get; set; }
        public string? Error { get; set; }
        public WhapiMessageDetails? Details { get; set; }
    }

    public class WhapiMessageDetails
    {
        public string? Ack { get; set; }
        public long Timestamp { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
    }

    public class WhapiMessageStatus
    {
        public string Id { get; set; } = string.Empty;
        public string? Ack { get; set; } // 0=sent, 1=delivered, 2=read, 3=played
        public long Timestamp { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? Type { get; set; }
        public WhapiMessageBody? Body { get; set; }
    }

    public class WhapiMessageBody
    {
        public string? Text { get; set; }
        public WhapiMediaInfo? Media { get; set; }
    }

    public class WhapiMediaInfo
    {
        public string? Url { get; set; }
        public string? MimeType { get; set; }
        public string? FileName { get; set; }
        public long Size { get; set; }
    }

    public class WhapiSettingsResponse
    {
        public string? Webhook { get; set; }
        public WhapiInstanceInfo? Instance { get; set; }
    }

    public class WhapiInstanceInfo
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public bool Connected { get; set; }
    }
}
