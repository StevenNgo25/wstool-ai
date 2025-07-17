using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
        {
            _groupService = groupService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<GroupDto>>> GetGroups([FromQuery] PaginationRequest request)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var groups = await _groupService.GetGroupsAsync(request, userId, userRole);
            return Ok(groups);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginatedResponse<GroupDto>>> SearchGroupsByInstances([FromBody] GroupSearchRequest request)
        {

            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var groups = await _groupService.SearchGroupsByInstancesAsync(request, userId, userRole);
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> GetGroup(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var stats = await _groupService.GetDashboardStatsAsync(userId, userRole);
            return Ok(stats);
        }
    }
}
