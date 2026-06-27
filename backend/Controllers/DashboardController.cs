using ComplaintManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {   
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        
        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult>
            GetUserDashboard()
        {
            var response =
                await _dashboardService
                    .GetUserDashboardAsync();

            return Ok(response);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult>
            GetAdminDashboard()
        {
            var result =
                await _dashboardService
                    .GetAdminDashboardAsync();

            return Ok(result);
        }
        
        [Authorize(Roles = "Employee")]
        [HttpGet("employee")]
        public async Task<IActionResult>
            GetEmployeeDashboard()
        {
            var result =
                await _dashboardService
                    .GetEmployeeDashboardAsync();

            return Ok(result);
        }
    }
    
}
