using System;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
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
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IComplaintPriorityRepository _priorityRepository;
        private readonly IEmployeeDesignationRepository _designationRepository;

        public AdminController(
            IEmployeeService employeeService, 
            IUserService userService,
            ICategoryRepository categoryRepository,
            IDepartmentRepository departmentRepository,
            IComplaintPriorityRepository priorityRepository,
            IEmployeeDesignationRepository designationRepository)
        {
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _priorityRepository = priorityRepository ?? throw new ArgumentNullException(nameof(priorityRepository));
            _designationRepository = designationRepository ?? throw new ArgumentNullException(nameof(designationRepository));
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

        // Categories CRUD
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] AdminCreateCategoryRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return BadRequest("Category name is required.");
            }

            var category = new ComplaintCategory
            {
                Categoryname = request.CategoryName
            };

            var created = await _categoryRepository.AddAsync(category);
            return CreatedAtAction(nameof(GetCategoryById), new { id = created.CategoryId }, created);
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] AdminUpdateCategoryRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return BadRequest("Category name is required.");
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Categoryname = request.CategoryName;
            await _categoryRepository.UpdateAsync(category);
            return Ok(category);
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await _categoryRepository.DeleteAsync(category);
            return Ok(new { Message = "Category deleted successfully." });
        }

        // Departments CRUD
        [HttpPost("departments")]
        public async Task<IActionResult> CreateDepartment([FromBody] AdminCreateDepartmentRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                return BadRequest("Department name is required.");
            }

            var department = new Department
            {
                DepartmentName = request.DepartmentName
            };

            var created = await _departmentRepository.AddAsync(department);
            return CreatedAtAction(nameof(GetDepartmentById), new { id = created.DepartmentId }, created);
        }

        [HttpGet("departments/{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }

        [HttpPut("departments/{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] AdminUpdateDepartmentRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                return BadRequest("Department name is required.");
            }

            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            department.DepartmentName = request.DepartmentName;
            await _departmentRepository.UpdateAsync(department);
            return Ok(department);
        }

        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            await _departmentRepository.DeleteAsync(department);
            return Ok(new { Message = "Department deleted successfully." });
        }

        // Priorities CRUD
        [HttpPost("priorities")]
        public async Task<IActionResult> CreatePriority([FromBody] AdminCreatePriorityRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PriorityName))
            {
                return BadRequest("Priority name is required.");
            }

            var priority = new ComplaintPriority
            {
                Priority = request.PriorityName
            };

            var created = await _priorityRepository.AddAsync(priority);
            return CreatedAtAction(nameof(GetPriorityById), new { id = created.PriorityId }, created);
        }

        [HttpGet("priorities/{id}")]
        public async Task<IActionResult> GetPriorityById(int id)
        {
            var priority = await _priorityRepository.GetByIdAsync(id);
            if (priority == null)
            {
                return NotFound();
            }
            return Ok(priority);
        }

        [HttpPut("priorities/{id}")]
        public async Task<IActionResult> UpdatePriority(int id, [FromBody] AdminUpdatePriorityRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PriorityName))
            {
                return BadRequest("Priority name is required.");
            }

            var priority = await _priorityRepository.GetByIdAsync(id);
            if (priority == null)
            {
                return NotFound();
            }

            priority.Priority = request.PriorityName;
            await _priorityRepository.UpdateAsync(priority);
            return Ok(priority);
        }

        [HttpDelete("priorities/{id}")]
        public async Task<IActionResult> DeletePriority(int id)
        {
            var priority = await _priorityRepository.GetByIdAsync(id);
            if (priority == null)
            {
                return NotFound();
            }

            await _priorityRepository.DeleteAsync(priority);
            return Ok(new { Message = "Priority deleted successfully." });
        }

        // Designations CRUD
        [HttpPost("designations")]
        public async Task<IActionResult> CreateDesignation([FromBody] AdminCreateDesignationRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.DesignationName))
            {
                return BadRequest("Designation name is required.");
            }

            var designation = new EmployeeDesignation
            {
                DesignationName = request.DesignationName,
                EscalationLevel = request.EscalationLevel
            };

            var created = await _designationRepository.AddAsync(designation);
            return CreatedAtAction(nameof(GetDesignationById), new { id = created.DesignationId }, created);
        }

        [HttpGet("designations/{id}")]
        public async Task<IActionResult> GetDesignationById(int id)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null)
            {
                return NotFound();
            }
            return Ok(designation);
        }

        [HttpPut("designations/{id}")]
        public async Task<IActionResult> UpdateDesignation(int id, [FromBody] AdminUpdateDesignationRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.DesignationName))
            {
                return BadRequest("Designation name is required.");
            }

            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null)
            {
                return NotFound();
            }

            designation.DesignationName = request.DesignationName;
            designation.EscalationLevel = request.EscalationLevel;
            await _designationRepository.UpdateAsync(designation);
            return Ok(designation);
        }

        [HttpDelete("designations/{id}")]
        public async Task<IActionResult> DeleteDesignation(int id)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null)
            {
                return NotFound();
            }

            await _designationRepository.DeleteAsync(designation);
            return Ok(new { Message = "Designation deleted successfully." });
        }
    }
}
