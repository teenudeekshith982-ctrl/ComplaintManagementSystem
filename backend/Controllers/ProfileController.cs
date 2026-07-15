using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly ComplaintManagementSystemContext _context;

        public ProfileController(
            ICurrentUserService currentUserService,
            IUserRepository userRepository,
            ComplaintManagementSystemContext context)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            int userId = _currentUserService.GetUserId();

            var user = await _context.Users
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            object employeeInfo = null;
            if (user.Employee != null)
            {
                employeeInfo = new
                {
                    user.Employee.EmployeeId,
                    user.Employee.DepartmentId,
                    DepartmentName = user.Employee.Department?.DepartmentName,
                    Designation = user.Employee.Designation.ToString()
                };
            }

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                user.Phone,
                user.Role,
                user.JoinedDate,
                EmployeeInfo = employeeInfo
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            int userId = _currentUserService.GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            // Check if phone already exists for another user
            var existingUserWithPhone = await _userRepository.GetByPhoneAsync(request.Phone);
            if (existingUserWithPhone != null && existingUserWithPhone.UserId != userId)
            {
                throw new ConflictException("Phone number already in use by another user.");
            }

            user.Name = request.Name;
            user.Phone = request.Phone;

            await _userRepository.UpdateAsync(user);

            return Ok(new
            {
                Message = "Profile updated successfully.",
                user.UserId,
                user.Name,
                user.Phone
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            int userId = _currentUserService.GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new BadRequestException("Invalid current password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            return Ok(new { Message = "Password changed successfully." });
        }
    }
}
