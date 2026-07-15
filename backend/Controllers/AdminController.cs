using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Controllers
{   
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserRepository _userRepository;

        public AdminController(IEmployeeService employeeService, IUserRepository userRepository)
        {
            _employeeService = employeeService;
            _userRepository = userRepository;
        }
        
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee(
            [FromBody] CreateEmployeeRequestDto request)
        {
            var response =
                await _employeeService
                    .CreateAsync(request);

            return Created(response.EmployeeId.ToString(), response);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? excludeRole = null)
        {
            var query = _userRepository.GetQueryable();
            if (!string.IsNullOrEmpty(excludeRole))
            {
                query = query.Where(u => u.Role != excludeRole);
            }

            var totalRecords = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.JoinedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    JoinedDate = u.JoinedDate
                })
                .ToListAsync();

            return Ok(new
            {
                Data = users,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpPatch("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(int id, [FromBody] ToggleUserActiveRequestDto? request = null)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            bool targetState;
            if (request?.IsActive.HasValue == true)
            {
                targetState = request.IsActive.Value;
                if (user.IsActive == targetState)
                {
                    return Ok(new { Message = $"User is already {(user.IsActive ? "activated" : "deactivated")}" });
                }
            }
            else
            {
                targetState = !user.IsActive;
            }

            user.IsActive = targetState;
            await _userRepository.UpdateAsync(user);

            return Ok(new { Message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully" });
        }
    }
}
