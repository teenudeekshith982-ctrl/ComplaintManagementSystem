using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EscalationController : ControllerBase
    {
        private readonly IEscalationService _escalationService;
        private readonly ComplaintManagementSystemContext _context;

        public EscalationController(
            IEscalationService escalationService,
            ComplaintManagementSystemContext context)
        {
            _escalationService = escalationService;
            _context = context;
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateEscalation(
            [FromBody] CreateEscalationRequestDto request)
        {
            var response = await _escalationService.CreateEscalationAsync(request);
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("auto-trigger")]
        public async Task<IActionResult> AutoEscalateComplaints()
        {
            await _escalationService.AutoEscalateComplaintsAsync();
            return Ok(new { Message = "Auto escalation completed successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetEscalations(
            [FromQuery] EscalationFilterDto filter)
        {
            var response = await _escalationService.GetEscalationsAsync(filter);
            return Ok(response);
        }

        [Authorize(Roles = "Admin,Employee,User")]
        [HttpGet("complaint/{complaintId}")]
        public async Task<IActionResult> GetComplaintEscalations(int complaintId)
        {
            var result = await _escalationService.GetComplaintEscalationsAsync(complaintId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/action")]
        public async Task<IActionResult> ResolveEscalation(int id, [FromBody] EscalationActionRequestDto request)
        {
            await _escalationService.ResolveEscalationAsync(id, request);
            return Ok(new { Message = $"Escalation successfully resolved with action: {request.Action}." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending-count")]
        public async Task<IActionResult> GetPendingCount()
        {
            var count = await _escalationService.GetPendingEscalationCountAsync();
            return Ok(new { count });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("eligible-employees")]
        public async Task<IActionResult> GetEligibleEmployees(
            [FromQuery] int complaintId,
            [FromQuery] string action)
        {
            var escalation = await _context.EscalatedComplaints
                .Include(e => e.Complaint)
                .Include(e => e.RequestedBy)
                .FirstOrDefaultAsync(e => e.EscalatedId == complaintId);

            if (escalation == null || escalation.Complaint == null)
            {
                return NotFound(new { message = "Escalation not found." });
            }

            int departmentId = escalation.Complaint.CategoryId;

            if (action.Equals("Accept", StringComparison.OrdinalIgnoreCase))
            {
                EmployeeDesignationEnum requiredDesignation;
                switch ((EscalationLevelEnum)escalation.EscalatedLevelId)
                {
                    case EscalationLevelEnum.TeamLead:
                        requiredDesignation = EmployeeDesignationEnum.TeamLead;
                        break;
                    case EscalationLevelEnum.Manager:
                        requiredDesignation = EmployeeDesignationEnum.Manager;
                        break;
                    case EscalationLevelEnum.SeniorManager:
                        requiredDesignation = EmployeeDesignationEnum.SeniorManager;
                        break;
                    default:
                        return BadRequest(new { message = "Invalid escalation level." });
                }

                var query = _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Department)
                    .Where(e => e.IsActive && e.User.IsActive
                        && e.Designation == requiredDesignation);

                if (requiredDesignation == EmployeeDesignationEnum.Employee || requiredDesignation == EmployeeDesignationEnum.TeamLead)
                {
                    query = query.Where(e => e.DepartmentId == departmentId);
                }
                else
                {
                    var managementDept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Management");
                    int managementDeptId = managementDept?.DepartmentId ?? 4;
                    query = query.Where(e => e.DepartmentId == managementDeptId);
                }

                var employees = await query
                    .Select(e => new
                    {
                        e.EmployeeId,
                        Name = e.User.Name,
                        Designation = e.Designation.ToString(),
                        DepartmentName = e.Department != null ? e.Department.DepartmentName : ""
                    })
                    .ToListAsync();

                return Ok(employees);
            }
            else if (action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                var requiredDesignation = escalation.RequestedBy?.Designation ?? EmployeeDesignationEnum.Employee;

                var query = _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Department)
                    .Where(e => e.IsActive && e.User.IsActive
                        && e.Designation == requiredDesignation);

                if (requiredDesignation == EmployeeDesignationEnum.Employee || requiredDesignation == EmployeeDesignationEnum.TeamLead)
                {
                    query = query.Where(e => e.DepartmentId == departmentId);
                }
                else
                {
                    var managementDept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Management");
                    int managementDeptId = managementDept?.DepartmentId ?? 4;
                    query = query.Where(e => e.DepartmentId == managementDeptId);
                }

                var employees = await query
                    .Select(e => new
                    {
                        e.EmployeeId,
                        Name = e.User.Name,
                        Designation = e.Designation.ToString(),
                        DepartmentName = e.Department != null ? e.Department.DepartmentName : ""
                    })
                    .ToListAsync();

                return Ok(employees);
            }

            return BadRequest(new { message = "Invalid action. Must be 'Accept' or 'Reject'." });
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("next-level/{complaintId}")]
        public async Task<IActionResult> GetNextEscalationLevel(int complaintId)
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

            if (complaint == null)
            {
                return NotFound(new { message = "Complaint not found." });
            }

            var existingEscalations = await _context.EscalatedComplaints
                .Where(e => e.ComplaintId == complaintId)
                .ToListAsync();

            int nextLevelId = existingEscalations.Count + 1;

            if (nextLevelId > (int)EscalationLevelEnum.SeniorManager)
            {
                return Ok(new { maxLevelReached = true, nextLevel = (string?)null, nextLevelId = 0 });
            }

            var levelName = await _context.EscalatedLevels
                .Where(l => l.EscalatedLevelId == nextLevelId)
                .Select(l => l.LevelName)
                .FirstOrDefaultAsync();

            return Ok(new { maxLevelReached = false, nextLevel = levelName, nextLevelId });
        }
    }
}
