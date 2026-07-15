using System;
using System.Linq;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ComplaintManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<PagedResponseDto<UserListItemDto>> GetUsersAsync(int pageNumber, int pageSize, string? excludeRole)
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

            return new PagedResponseDto<UserListItemDto>
            {
                Data = users,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task ToggleUserActiveAsync(int id, ToggleUserActiveRequestDto? request)
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
                    return; // Already in target state
                }
            }
            else
            {
                targetState = !user.IsActive;
            }

            user.IsActive = targetState;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetQueryable()
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Department)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Designation)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            ProfileEmployeeInfoDto? employeeInfo = null;
            if (user.Employee != null)
            {
                employeeInfo = new ProfileEmployeeInfoDto
                {
                    EmployeeId = user.Employee.EmployeeId,
                    DepartmentId = user.Employee.DepartmentId,
                    DepartmentName = user.Employee.Department?.DepartmentName ?? string.Empty,
                    Designation = user.Employee.Designation?.DesignationName ?? string.Empty
                };
            }

            return new UserProfileDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                JoinedDate = user.JoinedDate,
                EmployeeInfo = employeeInfo
            };
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
        {
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

            return await GetProfileAsync(userId);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
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
        }
    }
}
