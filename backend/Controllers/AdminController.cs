using System;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{   
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserService _userService;

        public AdminController(IEmployeeService employeeService, IUserService userService)
        {
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee(
            [FromBody] CreateEmployeeRequestDto request)
        {
            var response = await _employeeService.CreateAsync(request);
            return Created(response.EmployeeId.ToString(), response);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? excludeRole = null)
        {
            var result = await _userService.GetUsersAsync(pageNumber, pageSize, excludeRole);
            return Ok(result);
        }

        [HttpPatch("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(int id, [FromBody] ToggleUserActiveRequestDto? request = null)
        {
            await _userService.ToggleUserActiveAsync(id, request);
            return Ok(new { Message = "User active status toggled successfully." });
        }
    }
}
