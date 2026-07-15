using System;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{
    [Authorize]
    [Route("api/ai")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IComplaintAIService _complaintAIService;

        public AIController(IComplaintAIService complaintAIService)
        {
            _complaintAIService = complaintAIService ?? throw new ArgumentNullException(nameof(complaintAIService));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("complaints/{id}/category-validation")]
        public async Task<IActionResult> GetCategoryValidation(int id)
        {
            var result = await _complaintAIService.GetCategoryValidationAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("complaints/{id}/priority-recommendation")]
        public async Task<IActionResult> GetPriorityRecommendation(int id)
        {
            var result = await _complaintAIService.GetPriorityRecommendationAsync(id);
            return Ok(result);
        }

        [HttpGet("complaints/{id}/summary")]
        public async Task<IActionResult> GetComplaintSummary(int id)
        {
            var summary = await _complaintAIService.GetComplaintSummaryAsync(id);
            return Ok(new { summary });
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("complaints/{id}/similar")]
        public async Task<IActionResult> GetSimilarComplaints(int id)
        {
            var result = await _complaintAIService.GetSimilarComplaintsAsync(id);
            return Ok(result);
        }
    }
}
