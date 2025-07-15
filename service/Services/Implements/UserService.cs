using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Models;
using WhatsAppCampaignManager.Extensions;

namespace WhatsAppCampaignManager.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResponse<UserDto>> GetUsersAsync(PaginationRequest request)
        {
            var query = _context.AppUsers
                .Include(u => u.UserInstances)
                    .ThenInclude(ui => ui.Instance)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    AssignedInstances = u.UserInstances.Select(ui => new InstanceDto
                    {
                        Id = ui.Instance.Id,
                        Name = ui.Instance.Name,
                        WhatsAppNumber = ui.Instance.WhatsAppNumber,
                        WhapiUrl = ui.Instance.WhapiUrl,
                        IsActive = ui.Instance.IsActive,
                        CreatedAt = ui.Instance.CreatedAt,
                        UpdatedAt = ui.Instance.UpdatedAt
                    }).ToList()
                });

            // Apply search
            query = query.ApplySearch(request.Search, 
                x => x.Username, 
                x => x.Email);

            // Apply sorting
            query = query.ApplySort(request.SortBy, request.SortDirection);

            // If no sort specified, default to CreatedAt desc
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            return await query.ToPaginatedResponseAsync(request);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            return await _context.AppUsers
                .Include(u => u.UserInstances)
                    .ThenInclude(ui => ui.Instance)
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    AssignedInstances = u.UserInstances.Select(ui => new InstanceDto
                    {
                        Id = ui.Instance.Id,
                        Name = ui.Instance.Name,
                        WhatsAppNumber = ui.Instance.WhatsAppNumber,
                        WhapiUrl = ui.Instance.WhapiUrl,
                        IsActive = ui.Instance.IsActive,
                        CreatedAt = ui.Instance.CreatedAt,
                        UpdatedAt = ui.Instance.UpdatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Check if username already exists
                if (await _context.AppUsers.AnyAsync(u => u.Username == createUserDto.Username))
                {
                    _logger.LogWarning("Username already exists: {Username}", createUserDto.Username);
                    return null;
                }

                // Check if email already exists
                if (await _context.AppUsers.AnyAsync(u => u.Email == createUserDto.Email))
                {
                    _logger.LogWarning("Email already exists: {Email}", createUserDto.Email);
                    return null;
                }

                var user = new AppUser
                {
                    Username = createUserDto.Username,
                    Email = createUserDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                    Role = createUserDto.Role
                };

                _context.AppUsers.Add(user);
                await _context.SaveChangesAsync();

                return await GetUserByIdAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.AppUsers.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Username))
                {
                    // Check if username already exists (excluding current user)
                    if (await _context.AppUsers.AnyAsync(u => u.Username == updateUserDto.Username && u.Id != id))
                    {
                        _logger.LogWarning("Username already exists: {Username}", updateUserDto.Username);
                        return false;
                    }
                    user.Username = updateUserDto.Username;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Email))
                {
                    // Check if email already exists (excluding current user)
                    if (await _context.AppUsers.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id))
                    {
                        _logger.LogWarning("Email already exists: {Email}", updateUserDto.Email);
                        return false;
                    }
                    user.Email = updateUserDto.Email;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
                }

                if (!string.IsNullOrEmpty(updateUserDto.Role))
                {
                    user.Role = updateUserDto.Role;
                }

                if (updateUserDto.IsActive.HasValue)
                {
                    user.IsActive = updateUserDto.IsActive.Value;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.AppUsers.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                // Check if user has active jobs
                var hasActiveJobs = await _context.AppJobs
                    .AnyAsync(j => j.CreatedByUserId == id && j.Status == "Running");

                if (hasActiveJobs)
                {
                    _logger.LogWarning("Cannot delete user {UserId} with active jobs", id);
                    return false;
                }

                _context.AppUsers.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return false;
            }
        }

        public async Task<bool> AssignInstancesToUserAsync(int userId, AssignInstancesDto assignInstancesDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.AppUsers.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Remove existing assignments
                var existingAssignments = await _context.AppUserInstances
                    .Where(ui => ui.UserId == userId)
                    .ToListAsync();

                _context.AppUserInstances.RemoveRange(existingAssignments);

                // Add new assignments
                foreach (var instanceId in assignInstancesDto.InstanceIds)
                {
                    var instance = await _context.AppInstances.FindAsync(instanceId);
                    if (instance != null)
                    {
                        var userInstance = new AppUserInstance
                        {
                            UserId = userId,
                            InstanceId = instanceId,
                            CanSendMessages = assignInstancesDto.CanSendMessages,
                            CanCreateJobs = assignInstancesDto.CanCreateJobs
                        };

                        _context.AppUserInstances.Add(userInstance);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error assigning instances to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveInstanceFromUserAsync(int userId, int instanceId)
        {
            try
            {
                var userInstance = await _context.AppUserInstances
                    .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.InstanceId == instanceId);

                if (userInstance == null)
                {
                    return false;
                }

                _context.AppUserInstances.Remove(userInstance);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing instance {InstanceId} from user {UserId}", instanceId, userId);
                return false;
            }
        }

        public async Task<List<InstanceDto>> GetUserInstancesAsync(int userId)
        {
            return await _context.AppUserInstances
                .Include(ui => ui.Instance)
                .Where(ui => ui.UserId == userId)
                .Select(ui => new InstanceDto
                {
                    Id = ui.Instance.Id,
                    Name = ui.Instance.Name,
                    WhatsAppNumber = ui.Instance.WhatsAppNumber,
                    WhapiUrl = ui.Instance.WhapiUrl,
                    IsActive = ui.Instance.IsActive,
                    CreatedAt = ui.Instance.CreatedAt,
                    UpdatedAt = ui.Instance.UpdatedAt
                })
                .ToListAsync();
        }
        public async Task BulkDeleteUsersAsync(List<int> ids)
        {
            var usersToDelete = await _context.AppUsers
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            if (!usersToDelete.Any())
            {
                throw new KeyNotFoundException("No users found for the given IDs.");
            }

            // Check for jobs created by any of the users to be deleted
            var usersWithJobs = await _context.AppJobs
                .Where(j => ids.Contains(j.CreatedByUserId))
                .Select(j => j.CreatedByUserId)
                .Distinct()
                .ToListAsync();

            if (usersWithJobs.Any())
            {
                var usernamesWithJobs = await _context.AppUsers
                    .Where(u => usersWithJobs.Contains(u.Id))
                    .Select(u => u.Username)
                    .ToListAsync();
                throw new InvalidOperationException($"Cannot delete users: {string.Join(", ", usernamesWithJobs)} because they have created jobs. Please reassign or delete their jobs first.");
            }

            _context.AppUsers.RemoveRange(usersToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
