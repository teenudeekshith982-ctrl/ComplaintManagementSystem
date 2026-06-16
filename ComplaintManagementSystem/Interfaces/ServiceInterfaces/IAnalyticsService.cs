using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardSummaryDto>
        GetDashboardSummaryAsync();
    
    Task<List<ComplaintStatusAnalyticsDto>>
        GetComplaintsByStatusAsync();
}