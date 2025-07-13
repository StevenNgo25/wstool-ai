using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Models;
using WhatsAppCampaignManager.Extensions;

namespace WhatsAppCampaignManager.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<MessageService> _logger;

        public MessageService(ApplicationDbContext context, IFileService fileService, ILogger<MessageService> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<PaginatedResponse<MessageDto>> GetMessagesAsync(PaginationRequest request)
        {
            var query = _context.AppMessages
                .Include(m => m.CreatedByUser)
                .Include(m => m.Instance)
                .Include(m => m.MessageGroups)
                    .ThenInclude(mg => mg.Group)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    TextContent = m.TextContent,
                    ImageUrl = m.ImageUrl,
                    MessageType = m.MessageType,
                    CreatedByUserName = m.CreatedByUser.Username,
                    InstanceId = m.InstanceId,
                    InstanceName = m.Instance.Name,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    AssignedGroups = m.MessageGroups.Select(mg => new GroupDto
                    {
                        Id = mg.Group.Id,
                        GroupId = mg.Group.GroupId,
                        Name = mg.Group.Name,
                        Description = mg.Group.Description,
                        ParticipantCount = mg.Group.ParticipantCount,
                        IsActive = mg.Group.IsActive,
                        CreatedAt = mg.Group.CreatedAt,
                        LastSyncAt = mg.Group.LastSyncAt
                    }).ToList()
                });

            // Apply search
            query = query.ApplySearch(request.Search, 
                x => x.Title, 
                x => x.TextContent ?? "", 
                x => x.CreatedByUserName,
                x => x.InstanceName);

            // Apply sorting
            query = query.ApplySort(request.SortBy, request.SortDirection);

            // If no sort specified, default to CreatedAt desc
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            return await query.ToPaginatedResponseAsync(request);
        }

        public async Task<MessageDto?> GetMessageByIdAsync(int id)
        {
            return await _context.AppMessages
                .Include(m => m.CreatedByUser)
                .Include(m => m.Instance)
                .Include(m => m.MessageGroups)
                    .ThenInclude(mg => mg.Group)
                .Where(m => m.Id == id)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    TextContent = m.TextContent,
                    ImageUrl = m.ImageUrl,
                    MessageType = m.MessageType,
                    CreatedByUserName = m.CreatedByUser.Username,
                    InstanceId = m.InstanceId,
                    InstanceName = m.Instance.Name,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    AssignedGroups = m.MessageGroups.Select(mg => new GroupDto
                    {
                        Id = mg.Group.Id,
                        GroupId = mg.Group.GroupId,
                        Name = mg.Group.Name,
                        Description = mg.Group.Description,
                        ParticipantCount = mg.Group.ParticipantCount,
                        IsActive = mg.Group.IsActive,
                        CreatedAt = mg.Group.CreatedAt,
                        LastSyncAt = mg.Group.LastSyncAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<MessageDto?> CreateMessageAsync(CreateMessageDto createMessageDto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate user has access to instance
                var hasAccess = await _context.AppUserInstances
                    .AnyAsync(ui => ui.UserId == userId && ui.InstanceId == createMessageDto.InstanceId);

                if (!hasAccess)
                {
                    _logger.LogWarning("User {UserId} does not have access to instance {InstanceId}", userId, createMessageDto.InstanceId);
                    return null;
                }

                var message = new AppMessage
                {
                    Title = createMessageDto.Title,
                    TextContent = createMessageDto.TextContent,
                    MessageType = createMessageDto.MessageType,
                    InstanceId = createMessageDto.InstanceId,
                    CreatedByUserId = userId
                };

                _context.AppMessages.Add(message);
                await _context.SaveChangesAsync();

                // Assign to groups
                if (createMessageDto.GroupIds.Any())
                {
                    var messageGroups = createMessageDto.GroupIds.Select(groupId => new AppMessageGroup
                    {
                        MessageId = message.Id,
                        GroupId = groupId
                    }).ToList();

                    _context.AppMessageGroups.AddRange(messageGroups);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return await GetMessageByIdAsync(message.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating message");
                return null;
            }
        }

        public async Task<MessageDto?> CreateMessageWithFileAsync(CreateMessageWithFileDto createMessageDto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate user has access to instance
                var hasAccess = await _context.AppUserInstances
                    .AnyAsync(ui => ui.UserId == userId && ui.InstanceId == createMessageDto.InstanceId);

                if (!hasAccess)
                {
                    _logger.LogWarning("User {UserId} does not have access to instance {InstanceId}", userId, createMessageDto.InstanceId);
                    return null;
                }

                string? imageUrl = null;

                // Handle file upload
                if (createMessageDto.ImageFile != null)
                {
                    imageUrl = await _fileService.SaveFileAsync(createMessageDto.ImageFile);
                    if (imageUrl == null)
                    {
                        _logger.LogWarning("Failed to save uploaded file");
                        return null;
                    }
                }

                var message = new AppMessage
                {
                    Title = createMessageDto.Title,
                    TextContent = createMessageDto.TextContent,
                    ImageUrl = imageUrl,
                    MessageType = createMessageDto.MessageType,
                    InstanceId = createMessageDto.InstanceId,
                    CreatedByUserId = userId
                };

                _context.AppMessages.Add(message);
                await _context.SaveChangesAsync();

                // Assign to groups
                if (createMessageDto.GroupIds.Any())
                {
                    var messageGroups = createMessageDto.GroupIds.Select(groupId => new AppMessageGroup
                    {
                        MessageId = message.Id,
                        GroupId = groupId
                    }).ToList();

                    _context.AppMessageGroups.AddRange(messageGroups);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return await GetMessageByIdAsync(message.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating message with file");
                return null;
            }
        }

        public async Task<bool> UpdateMessageAsync(int id, UpdateMessageDto updateMessageDto, int userId, string userRole)
        {
            var message = await _context.AppMessages
                .Include(m => m.MessageGroups)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
            {
                return false;
            }

            // Check if user owns the message or is admin
            if (message.CreatedByUserId != userId && userRole != "Admin")
            {
                _logger.LogWarning("User {UserId} attempted to update message {MessageId} without permission", userId, id);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate instance access if changing instance
                if (updateMessageDto.InstanceId.HasValue && updateMessageDto.InstanceId != message.InstanceId)
                {
                    var hasAccess = await _context.AppUserInstances
                        .AnyAsync(ui => ui.UserId == userId && ui.InstanceId == updateMessageDto.InstanceId.Value);

                    if (!hasAccess && userRole != "Admin")
                    {
                        _logger.LogWarning("User {UserId} does not have access to instance {InstanceId}", userId, updateMessageDto.InstanceId);
                        return false;
                    }

                    message.InstanceId = updateMessageDto.InstanceId.Value;
                }

                // Update message properties
                if (!string.IsNullOrEmpty(updateMessageDto.Title))
                    message.Title = updateMessageDto.Title;

                if (updateMessageDto.TextContent != null)
                    message.TextContent = updateMessageDto.TextContent;

                if (!string.IsNullOrEmpty(updateMessageDto.MessageType))
                    message.MessageType = updateMessageDto.MessageType;

                message.UpdatedAt = DateTime.UtcNow;

                // Update group assignments if provided
                if (updateMessageDto.GroupIds != null)
                {
                    // Remove existing assignments
                    _context.AppMessageGroups.RemoveRange(message.MessageGroups);

                    // Add new assignments
                    if (updateMessageDto.GroupIds.Any())
                    {
                        var messageGroups = updateMessageDto.GroupIds.Select(groupId => new AppMessageGroup
                        {
                            MessageId = message.Id,
                            GroupId = groupId
                        }).ToList();

                        _context.AppMessageGroups.AddRange(messageGroups);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating message {MessageId}", id);
                return false;
            }
        }

        public async Task<bool> UpdateMessageWithFileAsync(int id, UpdateMessageWithFileDto updateMessageDto, int userId, string userRole)
        {
            var message = await _context.AppMessages
                .Include(m => m.MessageGroups)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
            {
                return false;
            }

            // Check if user owns the message or is admin
            if (message.CreatedByUserId != userId && userRole != "Admin")
            {
                _logger.LogWarning("User {UserId} attempted to update message {MessageId} without permission", userId, id);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate instance access if changing instance
                if (updateMessageDto.InstanceId.HasValue && updateMessageDto.InstanceId != message.InstanceId)
                {
                    var hasAccess = await _context.AppUserInstances
                        .AnyAsync(ui => ui.UserId == userId && ui.InstanceId == updateMessageDto.InstanceId.Value);

                    if (!hasAccess && userRole != "Admin")
                    {
                        _logger.LogWarning("User {UserId} does not have access to instance {InstanceId}", userId, updateMessageDto.InstanceId);
                        return false;
                    }

                    message.InstanceId = updateMessageDto.InstanceId.Value;
                }

                // Handle file operations
                if (updateMessageDto.RemoveImage && !string.IsNullOrEmpty(message.ImageUrl))
                {
                    // Delete old file
                    await _fileService.DeleteFileAsync(message.ImageUrl);
                    message.ImageUrl = null;
                }
                else if (updateMessageDto.ImageFile != null)
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(message.ImageUrl))
                    {
                        await _fileService.DeleteFileAsync(message.ImageUrl);
                    }

                    // Save new file
                    var newImageUrl = await _fileService.SaveFileAsync(updateMessageDto.ImageFile);
                    if (newImageUrl != null)
                    {
                        message.ImageUrl = newImageUrl;
                    }
                }

                // Update message properties
                if (!string.IsNullOrEmpty(updateMessageDto.Title))
                    message.Title = updateMessageDto.Title;

                if (updateMessageDto.TextContent != null)
                    message.TextContent = updateMessageDto.TextContent;

                if (!string.IsNullOrEmpty(updateMessageDto.MessageType))
                    message.MessageType = updateMessageDto.MessageType;

                message.UpdatedAt = DateTime.UtcNow;

                // Update group assignments if provided
                if (updateMessageDto.GroupIds != null)
                {
                    // Remove existing assignments
                    _context.AppMessageGroups.RemoveRange(message.MessageGroups);

                    // Add new assignments
                    if (updateMessageDto.GroupIds.Any())
                    {
                        var messageGroups = updateMessageDto.GroupIds.Select(groupId => new AppMessageGroup
                        {
                            MessageId = message.Id,
                            GroupId = groupId
                        }).ToList();

                        _context.AppMessageGroups.AddRange(messageGroups);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating message {MessageId} with file", id);
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id, int userId, string userRole)
        {
            try
            {
                var message = await _context.AppMessages.FindAsync(id);

                if (message == null)
                {
                    return false;
                }

                // Check if user owns the message or is admin
                if (message.CreatedByUserId != userId && userRole != "Admin")
                {
                    _logger.LogWarning("User {UserId} attempted to delete message {MessageId} without permission", userId, id);
                    return false;
                }

                // Check if message is being used in any active jobs
                var hasActiveJobs = await _context.AppJobs
                    .AnyAsync(j => j.MessageId == id && j.Status == "Running");

                if (hasActiveJobs)
                {
                    _logger.LogWarning("Cannot delete message {MessageId} that is being used in active jobs", id);
                    return false;
                }

                // Delete associated file
                if (!string.IsNullOrEmpty(message.ImageUrl))
                {
                    await _fileService.DeleteFileAsync(message.ImageUrl);
                }

                _context.AppMessages.Remove(message);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", id);
                return false;
            }
        }

        public async Task<bool> BulkDeleteMessagesAsync(List<int> messageIds, int userId, string userRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var messages = await _context.AppMessages
                    .Where(m => messageIds.Contains(m.Id))
                    .ToListAsync();

                var deletedCount = 0;
                foreach (var message in messages)
                {
                    // Check permissions
                    if (message.CreatedByUserId != userId && userRole != "Admin")
                    {
                        continue;
                    }

                    // Check if message is being used in active jobs
                    var hasActiveJobs = await _context.AppJobs
                        .AnyAsync(j => j.MessageId == message.Id && j.Status == "Running");

                    if (hasActiveJobs)
                    {
                        continue;
                    }

                    // Delete associated file
                    if (!string.IsNullOrEmpty(message.ImageUrl))
                    {
                        await _fileService.DeleteFileAsync(message.ImageUrl);
                    }

                    _context.AppMessages.Remove(message);
                    deletedCount++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk deleted {Count} messages out of {Total} requested", deletedCount, messageIds.Count);
                return deletedCount > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error bulk deleting messages");
                return false;
            }
        }

        public async Task<bool> UserCanAccessMessageAsync(int messageId, int userId, string userRole)
        {
            if (userRole == "Admin")
                return true;

            return await _context.AppMessages
                .AnyAsync(m => m.Id == messageId && m.CreatedByUserId == userId);
        }
    }
}
