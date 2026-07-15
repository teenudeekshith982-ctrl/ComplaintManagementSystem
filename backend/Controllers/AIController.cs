using System;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ComplaintManagementSystem.Controllers
{
    [Authorize]
    [Route("api/ai")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IComplaintRepository _complaintRepository;
        private readonly ILogger<AIController> _logger;

        public AIController(
            IAIService aiService,
            IComplaintRepository complaintRepository,
            ILogger<AIController> logger)
        {
            _aiService = aiService;
            _complaintRepository = complaintRepository;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("complaints/{id}/category-validation")]
        public async Task<IActionResult> GetCategoryValidation(int id)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(id);
                if (complaint == null) return NotFound(new { message = "Complaint not found" });

                var categoryName = complaint.ComplaintCategory?.Categoryname ?? "General";
                var result = await _aiService.ValidateCategoryAsync(complaint.Title, complaint.Description, categoryName);
                if (result == null) return StatusCode(503, new { message = "AI validation service is currently unavailable" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCategoryValidation for complaint {Id}", id);
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("complaints/{id}/priority-recommendation")]
        public async Task<IActionResult> GetPriorityRecommendation(int id)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(id);
                if (complaint == null) return NotFound(new { message = "Complaint not found" });

                var result = await _aiService.RecommendPriorityAsync(complaint.Title, complaint.Description);
                if (result == null) return StatusCode(503, new { message = "AI recommendation service is currently unavailable" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPriorityRecommendation for complaint {Id}", id);
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        [HttpGet("complaints/{id}/summary")]
        public async Task<IActionResult> GetComplaintSummary(int id)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(id);
                if (complaint == null) return NotFound(new { message = "Complaint not found" });

                var summary = await _aiService.SummarizeComplaintAsync(complaint.Title, complaint.Description);
                if (summary == null) return StatusCode(503, new { message = "AI summary service is currently unavailable" });

                return Ok(new { summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetComplaintSummary for complaint {Id}", id);
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("complaints/{id}/similar")]
        public async Task<IActionResult> GetSimilarComplaints(int id)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(id);
                if (complaint == null) return NotFound(new { message = "Complaint not found" });

                var otherComplaints = await _complaintRepository.GetResolvedOrClosedComplaintsAsync(id);
                var result = await _aiService.FindSimilarComplaintsAsync(id, complaint.Title, complaint.Description, otherComplaints);
                if (result == null) return StatusCode(503, new { message = "AI similarity service is currently unavailable" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSimilarComplaints for complaint {Id}", id);
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }
    }
}
