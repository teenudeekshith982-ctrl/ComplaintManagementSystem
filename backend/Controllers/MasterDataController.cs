using System;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterDataService;

        public MasterDataController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService ?? throw new ArgumentNullException(nameof(masterDataService));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _masterDataService.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("priorities")]
        public async Task<IActionResult> GetPriorities()
        {
            var priorities = await _masterDataService.GetPrioritiesAsync();
            return Ok(priorities);
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _masterDataService.GetStatusesAsync();
            return Ok(statuses);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _masterDataService.GetDepartmentsAsync();
            return Ok(departments);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] int? departmentId)
        {
            var employees = await _masterDataService.GetEmployeesAsync(departmentId);
            return Ok(employees);
        }
    }
}
