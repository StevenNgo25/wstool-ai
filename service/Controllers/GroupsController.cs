using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
        {
            _groupService = groupService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<GroupDto>>> GetGroups([FromQuery] PaginationRequest request)
        {
            var groups = await _groupService.GetGroupsAsync(request);
            return Ok(groups);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginatedResponse<GroupDto>>> SearchGroupsByInstances([FromBody] GroupSearchRequest request)
        {
            var groups = await _groupService.SearchGroupsByInstancesAsync(request);
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
            var stats = await _groupService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}
