using WhatsAppCampaignManager.DTOs;

namespace WhatsAppCampaignManager.Services
{
    public interface IInstanceService
    {
        Task<PaginatedResponse<InstanceDto>> GetInstancesAsync(PaginationRequest request);
        Task<InstanceDto?> GetInstanceByIdAsync(int id);
        Task<InstanceDto?> CreateInstanceAsync(CreateInstanceDto createInstanceDto);
        Task<bool> UpdateInstanceAsync(int id, UpdateInstanceDto updateInstanceDto);
        Task<bool> DeleteInstanceAsync(int id);
        Task<bool> AssignUserToInstanceAsync(int instanceId, AssignUserDto assignUserDto);
        Task<bool> UserHasAccessToInstanceAsync(int userId, int instanceId);


        Task LogoutInstanceAsync(int instanceId); // Renamed from DisconnectInstanceAsync
        Task<string?> GetQrCodeBase64Async(int instanceId);
        Task<string> GetConnectCodeAsync(int instanceId, string sdt); // New method
    }
}
