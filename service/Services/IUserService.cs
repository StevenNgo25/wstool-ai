using WhatsAppCampaignManager.DTOs;

namespace WhatsAppCampaignManager.Services
{
    public interface IUserService
    {
        Task<PaginatedResponse<UserDto>> GetUsersAsync(PaginationRequest request);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AssignInstancesToUserAsync(int userId, AssignInstancesDto assignInstancesDto);
        Task<bool> RemoveInstanceFromUserAsync(int userId, int instanceId);
        Task<List<InstanceDto>> GetUserInstancesAsync(int userId);
    }
}
