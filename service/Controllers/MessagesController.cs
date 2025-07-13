using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<MessageDto>>> GetMessages([FromQuery] PaginationRequest request)
        {
            var messages = await _messageService.GetMessagesAsync(request);
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDto>> GetMessage(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var message = await _messageService.CreateMessageAsync(createMessageDto, userId);
            if (message == null)
            {
                return BadRequest("Error creating message or no access to specified instance");
            }

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }

        [HttpPost("with-file")]
        public async Task<ActionResult<MessageDto>> CreateMessageWithFile([FromForm] CreateMessageWithFileDto createMessageDto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var message = await _messageService.CreateMessageWithFileAsync(createMessageDto, userId);
            if (message == null)
            {
                return BadRequest("Error creating message, invalid file, or no access to specified instance");
            }

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageDto updateMessageDto)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            var success = await _messageService.UpdateMessageAsync(id, updateMessageDto, userId, userRole);
            if (!success)
            {
                return BadRequest("Failed to update message. Check if message exists and you have permission.");
            }

            return NoContent();
        }

        [HttpPut("{id}/with-file")]
        public async Task<IActionResult> UpdateMessageWithFile(int id, [FromForm] UpdateMessageWithFileDto updateMessageDto)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            var success = await _messageService.UpdateMessageWithFileAsync(id, updateMessageDto, userId, userRole);
            if (!success)
            {
                return BadRequest("Failed to update message. Check if message exists and you have permission.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            var success = await _messageService.DeleteMessageAsync(id, userId, userRole);
            if (!success)
            {
                return BadRequest("Failed to delete message. Check if message exists, you have permission, and it's not being used in active jobs.");
            }

            return NoContent();
        }

        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDeleteMessages([FromBody] BulkDeleteRequest request)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            var success = await _messageService.BulkDeleteMessagesAsync(request.Ids, userId, userRole);
            if (!success)
            {
                return BadRequest("Failed to delete messages. Some messages may not exist or you don't have permission.");
            }

            return Ok();
        }
    }
}
