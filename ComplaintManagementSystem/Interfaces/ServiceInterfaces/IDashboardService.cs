using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IDashboardService
{
    Task<UserDashboardDto>
        GetUserDashboardAsync();
    
    Task<AdminDashboardDto>
        GetAdminDashboardAsync();
    
    Task<EmployeeDashboardDto>
        GetEmployeeDashboardAsync();
}