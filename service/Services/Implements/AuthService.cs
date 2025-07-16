using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Models;

namespace WhatsAppCampaignManager.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for username: {Username}", loginDto.Username);
                    return null;
                }

                // Update last login
                user.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return new LoginResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = DateTime.Now.AddHours(24),
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", loginDto.Username);
                return null;
            }
        }

        public async Task<LoginResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if username already exists
                if (await _context.AppUsers.AnyAsync(u => u.Username == registerDto.Username))
                {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", registerDto.Username);
                    return null;
                }

                // Check if email already exists
                if (await _context.AppUsers.AnyAsync(u => u.Email == registerDto.Email))
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", registerDto.Email);
                    return null;
                }

                var user = new AppUser
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = "User" // Default role
                };

                _context.AppUsers.Add(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return new LoginResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = DateTime.Now.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", registerDto.Username);
                return null;
            }
        }

        public async Task<AppUser?> GetUserByIdAsync(int userId)
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _context.AppUsers.FindAsync(userId);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user {UserId}", userId);
                return false;
            }
        }

        private string GenerateJwtToken(AppUser user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.Now.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
