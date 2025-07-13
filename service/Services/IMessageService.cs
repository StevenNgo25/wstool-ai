using WhatsAppCampaignManager.DTOs;

namespace WhatsAppCampaignManager.Services
{
    public interface IMessageService
    {
        Task<PaginatedResponse<MessageDto>> GetMessagesAsync(PaginationRequest request);
        Task<MessageDto?> GetMessageByIdAsync(int id);
        Task<MessageDto?> CreateMessageAsync(CreateMessageDto createMessageDto, int userId);
        Task<MessageDto?> CreateMessageWithFileAsync(CreateMessageWithFileDto createMessageDto, int userId);
        Task<bool> UpdateMessageAsync(int id, UpdateMessageDto updateMessageDto, int userId, string userRole);
        Task<bool> UpdateMessageWithFileAsync(int id, UpdateMessageWithFileDto updateMessageDto, int userId, string userRole);
        Task<bool> DeleteMessageAsync(int id, int userId, string userRole);
        Task<bool> BulkDeleteMessagesAsync(List<int> messageIds, int userId, string userRole);
        Task<bool> UserCanAccessMessageAsync(int messageId, int userId, string userRole);
    }
}
