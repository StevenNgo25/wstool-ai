using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsAppCampaignManager.DTOs;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InstancesController : ControllerBase
    {
        private readonly IInstanceService _instanceService;
        private readonly ILogger<InstancesController> _logger;

        public InstancesController(IInstanceService instanceService, ILogger<InstancesController> logger)
        {
            _instanceService = instanceService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<InstanceDto>>> GetInstances([FromQuery] PaginationRequest request)
        {
            var instances = await _instanceService.GetInstancesAsync(request);
            return Ok(instances);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InstanceDto>> GetInstance(int id)
        {
            var instance = await _instanceService.GetInstanceByIdAsync(id);
            if (instance == null)
            {
                return NotFound();
            }
            return Ok(instance);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<InstanceDto>> CreateInstance(CreateInstanceDto createInstanceDto)
        {
            var instance = await _instanceService.CreateInstanceAsync(createInstanceDto);
            if (instance == null)
            {
                return BadRequest("Invalid WHAPI token or WhatsApp number already exists");
            }
            return CreatedAtAction(nameof(GetInstance), new { id = instance.Id }, instance);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInstance(int id, UpdateInstanceDto updateInstanceDto)
        {
            var success = await _instanceService.UpdateInstanceAsync(id, updateInstanceDto);
            if (!success)
            {
                return BadRequest("Failed to update instance. Check if instance exists and token is valid.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteInstance(int id)
        {
            var success = await _instanceService.DeleteInstanceAsync(id);
            if (!success)
            {
                return BadRequest("Cannot delete instance. Check if instance exists or has active jobs.");
            }
            return NoContent();
        }

        [HttpPost("{id}/assign-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignUserToInstance(int id, [FromBody] AssignUserDto assignUserDto)
        {
            var success = await _instanceService.AssignUserToInstanceAsync(id, assignUserDto);
            if (!success)
            {
                return BadRequest("Failed to assign user to instance. Check if both user and instance exist and assignment doesn't already exist.");
            }
            return Ok();
        }

        [HttpGet("{id}/qrcode-base64")]
        public async Task<ActionResult<string>> GetQrCodeBase64(int id)
        {
            try
            {
                var qrCodeBase64 = await _instanceService.GetQrCodeBase64Async(id);
                if (string.IsNullOrEmpty(qrCodeBase64))
                {
                    return NotFound("QR Code not available for this instance or instance is not in connecting state.");
                }
                return Ok(qrCodeBase64);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}/connect-code")] // New endpoint
        public async Task<ActionResult<string>> GetConnectCode(int id)
        {
            try
            {
                var connectCode = await _instanceService.GetConnectCodeAsync(id);
                return Ok(connectCode);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{id}/logout")]
        public async Task<IActionResult> LogoutInstance(int id)
        {
            try
            {
                await _instanceService.LogoutInstanceAsync(id);
                return Ok("Instance logged out successfully.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
