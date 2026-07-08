using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly ICommentService _commentService;
        private readonly IWebHostEnvironment _env;

        public ComplaintsController(IComplaintService complaintService,
            ICommentService commentService,
            IWebHostEnvironment env)
        {
            _complaintService = complaintService;
            _commentService = commentService;
            _env = env;
        }
        
        [Authorize(Roles="Admin,User")]
        [HttpPost]
        public async Task<IActionResult> CreateComplaint(
            [FromForm] CreateComplaintRequestDto request)
        {
        
            var response = await _complaintService.CreateAsync(request);

            return Ok(response);
        }
        
        [Authorize(Roles="Admin,User")]
        [HttpGet("GetUserComplaints")]
        public async Task<IActionResult> GetMyComplaints(
           [FromQuery] GetMyComplaintRequestDto request)
        {
            var response =
                await _complaintService
                    .GetMyComplaintsAsync(request);

            return Ok(response);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetComplaints(
            [FromQuery] GetComplaintRequestDto request)
        {
            var result = await _complaintService
                .GetComplaintsAsync(request);

            return Ok(result);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/priority")]
        public async Task<IActionResult> AssignPriority(
            int id,
            [FromBody] AssignPriorityRequestDto request)
        {
            await _complaintService.AssignPriorityAsync(
                id,
                request);

            return Ok(new
            {
                Message = "Priority assigned successfully"
            });
        }
        
        
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignComplaint(
            int id)
        {
            var response =
                await _complaintService
                    .AssignComplaintAsync(id);

            return Ok(response);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/assign/{employeeId}")]
        public async Task<IActionResult> AssignComplaintToEmployee(
            int id,
            int employeeId)
        {
            await _complaintService
                .AssignComplaintToEmployeeAsync(
                    id,
                    employeeId);

            return Ok(new
            {
                Message = "Complaint assigned successfully"
            });
        }
        
        [Authorize(Roles = "Admin,Employee,User")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateComplaintStatus(
            int id,
            [FromBody] UpdateComplaintStatusRequestDto request)
        {
            await _complaintService
                .UpdateComplaintStatusAsync(
                    id,
                    request);

            return Ok(new
            {
                Message = "Complaint status updated successfully"
            });
        }
        
        [Authorize]
        [HttpGet("{complaintId}/tracking")]
        public async Task<IActionResult>
            GetComplaintTracking(int complaintId)
        {
            var result =
                await _complaintService
                    .GetComplaintTrackingAsync(complaintId);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}/complaintdetails")]
        public async Task<IActionResult> GetComplaintDetails(int id)
        {
            var result = await _complaintService.GetComplaintDetailsById(id);
            return Ok(result);
        }
        
        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<IActionResult>
            AddComment(
                int id,
                [FromBody] CommentRequestDto request)
        {
            var response =
                await _commentService
                    .AddCommentAsync(
                        id,
                        request);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id}/attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            var (filePath, fileName) = await _complaintService.GetAttachmentAsync(id, attachmentId);
            
            var fullPath = Path.Combine(_env.WebRootPath, filePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("Physical file not found on server.");
            }

            var contentType = GetMimeType(fileName);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, contentType, fileName);
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }
    }
}
    
   