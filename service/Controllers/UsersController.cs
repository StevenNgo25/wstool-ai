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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResponse<UserDto>>> GetUsers([FromQuery] PaginationRequest request)
        {
            var users = await _userService.GetUsersAsync(request);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Users can only view their own profile unless they're admin
            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            var user = await _userService.CreateUserAsync(createUserDto);
            if (user == null)
            {
                return BadRequest("Username or email already exists");
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Users can only update their own profile unless they're admin
            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            // Non-admin users cannot change role
            if (userRole != "Admin" && !string.IsNullOrEmpty(updateUserDto.Role))
            {
                updateUserDto.Role = null;
            }

            var success = await _userService.UpdateUserAsync(id, updateUserDto);
            if (!success)
            {
                return BadRequest("Failed to update user. Check if user exists and data is valid.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return BadRequest("Cannot delete user. Check if user exists or has active jobs.");
            }

            return NoContent();
        }

        [HttpPost("{id}/assign-instances")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignInstancesToUser(int id, [FromBody] AssignInstancesDto assignInstancesDto)
        {
            var success = await _userService.AssignInstancesToUserAsync(id, assignInstancesDto);
            if (!success)
            {
                return BadRequest("Failed to assign instances to user. Check if user exists.");
            }

            return Ok();
        }

        [HttpDelete("{userId}/instances/{instanceId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveInstanceFromUser(int userId, int instanceId)
        {
            var success = await _userService.RemoveInstanceFromUserAsync(userId, instanceId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id}/instances")]
        public async Task<ActionResult<List<InstanceDto>>> GetUserInstances(int id)
        {
            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Users can only view their own instances unless they're admin
            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            var instances = await _userService.GetUserInstancesAsync(id);
            return Ok(instances);
        }
    }
}
