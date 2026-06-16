using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IDashboardRepository
{
    
    public Task<UserDashboardDto>
        GetUserDashboardAsync(
            int userId);
}