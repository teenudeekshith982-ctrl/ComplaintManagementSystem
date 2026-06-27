using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ComplaintManagementSystemContext _context;
    
    
    public DashboardRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }
    
    public async Task<UserDashboardDto>
        GetUserDashboardAsync(
            int userId)
    {
        var complaints =
            _context.Complaints
                .Where(c =>
                    c.UserId == userId);

        return new UserDashboardDto
        {
            TotalComplaints =
                await complaints.CountAsync(),

            OpenComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.Open),

            AssignedComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.Assigned),

            InProgressComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.InProgress),

            ResolvedComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.Resolved),

            ClosedComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.Closed)
        };
    }
    
    public async Task<EmployeeDashboardDto>
        GetEmployeeDashboardAsync(
            int employeeId)
    {
        var complaints =
            _context.Complaints
                .Where(c =>
                    c.EmployeeId == employeeId);

        return new EmployeeDashboardDto
        {
            AssignedComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.Assigned),

            InProgressComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.InProgress),

            ResolvedComplaints =
                await complaints.CountAsync(
                    c => c.StatusId ==
                         (int)ComplaintStatusEnum.Resolved),

            EscalatedComplaints =
                await _context.EscalatedComplaints
                    .CountAsync(e =>
                        e.Complaint!.EmployeeId ==
                        employeeId),

            OverdueComplaints =
                await complaints.CountAsync(
                    c =>
                        c.DueDate < DateTime.UtcNow &&
                        c.StatusId !=
                        (int)ComplaintStatusEnum.Resolved &&
                        c.StatusId !=
                        (int)ComplaintStatusEnum.Closed)
        };
    }
    
    

}