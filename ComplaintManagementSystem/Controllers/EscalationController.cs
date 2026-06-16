using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EscalationController : ControllerBase
    {
        private readonly IEscalationService _escalationService;

        public EscalationController(
            IEscalationService escalationService)
        {
            _escalationService = escalationService;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateEscalation(
            [FromBody] CreateEscalationRequestDto request)
        {
            var response =
                await _escalationService
                    .CreateEscalationAsync(request);

            return Ok(response);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("auto-trigger")]
        public async Task<IActionResult>
            AutoEscalateComplaints()
        {
            await _escalationService
                .AutoEscalateComplaintsAsync();

            return Ok(new
            {
                Message =
                    "Auto escalation completed successfully"
            });
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult>
            GetEscalations(
                [FromQuery]
                EscalationFilterDto filter)
        {
            var response =
                await _escalationService
                    .GetEscalationsAsync(
                        filter);

            return Ok(response);
        }
    }
}
