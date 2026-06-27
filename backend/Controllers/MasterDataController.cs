using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly ComplaintManagementSystemContext _context;

        public MasterDataController(ComplaintManagementSystemContext context)
        {
            _context = context;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.ComplaintCategories
                .Select(c => new
                {
                    c.CategoryId,
                    CategoryName = c.Categoryname
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("priorities")]
        public async Task<IActionResult> GetPriorities()
        {
            var priorities = await _context.ComplaintPriorities
                .Include(p => p.SLA)
                .Select(p => new
                {
                    p.PriorityId,
                    PriorityName = p.Priority,
                    SlaResolutionHours = p.SLA != null ? p.SLA.ResolutionHours : 0
                })
                .ToListAsync();

            return Ok(priorities);
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _context.ComplaintStatuses
                .Select(s => new
                {
                    s.StatusId,
                    s.StatusName
                })
                .ToListAsync();

            return Ok(statuses);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new
                {
                    d.DepartmentId,
                    d.DepartmentName
                })
                .ToListAsync();

            return Ok(departments);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] int? departmentId)
        {
            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .Where(e => e.IsActive && e.User.IsActive);

            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            var employees = await query
                .Select(e => new
                {
                    e.EmployeeId,
                    EmployeeName = e.User.Name,
                    DepartmentName = e.Department.DepartmentName,
                    Designation = e.Designation.ToString()
                })
                .ToListAsync();

            return Ok(employees);
        }
    }
}
