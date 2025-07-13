using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Models;
using WhatsAppCampaignManager.Extensions;

namespace WhatsAppCampaignManager.Services
{
    public class InstanceService : IInstanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWhapiService _whapiService;
        private readonly ILogger<InstanceService> _logger;

        public InstanceService(ApplicationDbContext context, IWhapiService whapiService, ILogger<InstanceService> logger)
        {
            _context = context;
            _whapiService = whapiService;
            _logger = logger;
        }

        public async Task<PaginatedResponse<InstanceDto>> GetInstancesAsync(PaginationRequest request)
        {
            var query = _context.AppInstances
                .Select(i => new InstanceDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    WhatsAppNumber = i.WhatsAppNumber,
                    WhapiUrl = i.WhapiUrl,
                    IsActive = i.IsActive,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                });

            // Apply search
            query = query.ApplySearch(request.Search, 
                x => x.Name, 
                x => x.WhatsAppNumber);

            // Apply sorting
            query = query.ApplySort(request.SortBy, request.SortDirection);

            // If no sort specified, default to CreatedAt desc
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            return await query.ToPaginatedResponseAsync(request);
        }

        public async Task<InstanceDto?> GetInstanceByIdAsync(int id)
        {
            return await _context.AppInstances
                .Where(i => i.Id == id)
                .Select(i => new InstanceDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    WhatsAppNumber = i.WhatsAppNumber,
                    WhapiUrl = i.WhapiUrl,
                    IsActive = i.IsActive,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<InstanceDto?> CreateInstanceAsync(CreateInstanceDto createInstanceDto)
        {
            try
            {
                // Validate token with WHAPI
                var isValidToken = await _whapiService.ValidateTokenAsync(createInstanceDto.WhapiToken, createInstanceDto.WhapiUrl);
                if (!isValidToken)
                {
                    _logger.LogWarning("Invalid WHAPI token provided for instance creation");
                    return null;
                }

                // Check if WhatsApp number already exists
                var existingInstance = await _context.AppInstances
                    .FirstOrDefaultAsync(i => i.WhatsAppNumber == createInstanceDto.WhatsAppNumber);
                
                if (existingInstance != null)
                {
                    _logger.LogWarning("WhatsApp number {Number} already exists", createInstanceDto.WhatsAppNumber);
                    return null;
                }

                var instance = new AppInstance
                {
                    Name = createInstanceDto.Name,
                    WhatsAppNumber = createInstanceDto.WhatsAppNumber,
                    WhapiToken = createInstanceDto.WhapiToken,
                    WhapiUrl = createInstanceDto.WhapiUrl
                };

                _context.AppInstances.Add(instance);
                await _context.SaveChangesAsync();

                return new InstanceDto
                {
                    Id = instance.Id,
                    Name = instance.Name,
                    WhatsAppNumber = instance.WhatsAppNumber,
                    WhapiUrl = instance.WhapiUrl,
                    IsActive = instance.IsActive,
                    CreatedAt = instance.CreatedAt,
                    UpdatedAt = instance.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating instance");
                return null;
            }
        }

        public async Task<bool> UpdateInstanceAsync(int id, UpdateInstanceDto updateInstanceDto)
        {
            try
            {
                var instance = await _context.AppInstances.FindAsync(id);
                if (instance == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(updateInstanceDto.Name))
                    instance.Name = updateInstanceDto.Name;

                if (!string.IsNullOrEmpty(updateInstanceDto.WhapiToken))
                {
                    // Validate new token
                    var isValidToken = await _whapiService.ValidateTokenAsync(updateInstanceDto.WhapiToken, updateInstanceDto.WhapiUrl ?? instance.WhapiUrl);
                    if (!isValidToken)
                    {
                        _logger.LogWarning("Invalid WHAPI token provided for instance update");
                        return false;
                    }
                    instance.WhapiToken = updateInstanceDto.WhapiToken;
                }

                if (!string.IsNullOrEmpty(updateInstanceDto.WhapiUrl))
                    instance.WhapiUrl = updateInstanceDto.WhapiUrl;

                if (updateInstanceDto.IsActive.HasValue)
                    instance.IsActive = updateInstanceDto.IsActive.Value;

                instance.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating instance {InstanceId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteInstanceAsync(int id)
        {
            try
            {
                var instance = await _context.AppInstances.FindAsync(id);
                if (instance == null)
                {
                    return false;
                }

                // Check if instance is being used in any jobs
                var hasActiveJobs = await _context.AppJobs
                    .AnyAsync(j => j.InstanceId == id && j.Status == "Running");

                if (hasActiveJobs)
                {
                    _logger.LogWarning("Cannot delete instance {InstanceId} with active jobs", id);
                    return false;
                }

                _context.AppInstances.Remove(instance);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting instance {InstanceId}", id);
                return false;
            }
        }

        public async Task<bool> AssignUserToInstanceAsync(int instanceId, AssignUserDto assignUserDto)
        {
            try
            {
                var instance = await _context.AppInstances.FindAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                var user = await _context.AppUsers.FindAsync(assignUserDto.UserId);
                if (user == null)
                {
                    return false;
                }

                // Check if assignment already exists
                var existingAssignment = await _context.AppUserInstances
                    .FirstOrDefaultAsync(ui => ui.UserId == assignUserDto.UserId && ui.InstanceId == instanceId);

                if (existingAssignment != null)
                {
                    _logger.LogWarning("User {UserId} is already assigned to instance {InstanceId}", assignUserDto.UserId, instanceId);
                    return false;
                }

                var userInstance = new AppUserInstance
                {
                    UserId = assignUserDto.UserId,
                    InstanceId = instanceId,
                    CanSendMessages = assignUserDto.CanSendMessages,
                    CanCreateJobs = assignUserDto.CanCreateJobs
                };

                _context.AppUserInstances.Add(userInstance);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user to instance");
                return false;
            }
        }

        public async Task<bool> UserHasAccessToInstanceAsync(int userId, int instanceId)
        {
            return await _context.AppUserInstances
                .AnyAsync(ui => ui.UserId == userId && ui.InstanceId == instanceId);
        }
    }
}
