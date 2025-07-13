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
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly ILogger<JobsController> _logger;

        public JobsController(IJobService jobService, ILogger<JobsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<JobDto>>> GetJobs([FromQuery] PaginationRequest request)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Admin can see all jobs, members can only see their own
            var jobs = await _jobService.GetJobsAsync(request, userRole == "Admin" ? null : userId);
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobDto>> GetJob(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Check if user can access this job
            if (userRole != "Admin" && job.CreatedByUserName != User.Identity?.Name)
            {
                return Forbid();
            }

            return Ok(job);
        }

        [HttpPost]
        public async Task<ActionResult<JobDto>> CreateJob(CreateJobDto createJobDto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var job = await _jobService.CreateJobAsync(createJobDto, userId);
            if (job == null)
            {
                return BadRequest("Failed to create job. Check message and instance access.");
            }

            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }

        [HttpPost("{id}/restart")]
        public async Task<IActionResult> RestartJob(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            var success = await _jobService.RestartJobAsync(id, userId, userRole);
            if (!success)
            {
                return BadRequest("Failed to restart job. Check if job exists, you have permission, and job can be restarted.");
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _jobService.DeleteJobAsync(id, userId);
            
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDeleteJobs([FromBody] BulkDeleteJobsRequest request)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            var success = await _jobService.BulkDeleteJobsAsync(request.JobIds, userId, userRole);
            if (!success)
            {
                return BadRequest("Failed to delete jobs. Some jobs may not exist or you don't have permission.");
            }

            return Ok();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelJob(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Check if user can cancel this job
            if (userRole != "Admin" && job.CreatedByUserName != User.Identity?.Name)
            {
                return Forbid();
            }

            if (job.Status != "Pending" && job.Status != "Running")
            {
                return BadRequest("Can only cancel pending or running jobs");
            }

            var success = await _jobService.UpdateJobStatusAsync(id, "Cancelled");
            if (!success)
            {
                return StatusCode(500, "Failed to cancel job");
            }

            return Ok();
        }
    }
}
