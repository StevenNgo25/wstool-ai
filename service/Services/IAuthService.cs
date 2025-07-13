using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Models;

namespace WhatsAppCampaignManager.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<LoginResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AppUser?> GetUserByIdAsync(int userId);
        Task<bool> UpdateLastLoginAsync(int userId);
    }
}
