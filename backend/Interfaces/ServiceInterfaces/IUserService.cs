using System.Threading.Tasks;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponseDto<UserListItemDto>> GetUsersAsync(int pageNumber, int pageSize, string? excludeRole);
        Task ToggleUserActiveAsync(int id, ToggleUserActiveRequestDto? request);
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    }
}
