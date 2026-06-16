using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{   
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        
        private readonly IEmployeeService _employeeService;

        public AdminController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
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
    }
}
