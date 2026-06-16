using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Services;

public class AnalyticsService : IAnalyticsService
{   
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<AnalyticsService> _logger;
    public AnalyticsService(IAnalyticsRepository analyticsRepository, ILogger<AnalyticsService> logger)
    {
        analyticsRepository = _analyticsRepository;
        _logger = logger;
    }
    
    public async Task<DashboardSummaryDto>
        GetDashboardSummaryAsync()
    {
        _logger.LogInformation(
            "Getting dashboard analytics");

        var result =
            await _analyticsRepository
                .GetDashboardSummaryAsync();

        _logger.LogInformation(
            "Dashboard analytics retrieved successfully");

        return result;
    }
    
    public async Task<List<ComplaintStatusAnalyticsDto>>
        GetComplaintsByStatusAsync()
    {
        _logger.LogInformation(
            "Getting complaint status analytics");

        return await _analyticsRepository
            .GetComplaintsByStatusAsync();
    }
}