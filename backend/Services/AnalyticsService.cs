using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Services;

public class AnalyticsService : IAnalyticsService
{   
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<AnalyticsService> _logger;
    public AnalyticsService(IAnalyticsRepository analyticsRepository, ILogger<AnalyticsService> logger)
    {
        _analyticsRepository = analyticsRepository;
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

    public async Task<List<ComplaintCategoryAnalyticsDto>> GetComplaintsByCategoryAsync()
    {
        _logger.LogInformation("Getting complaint category analytics");
        return await _analyticsRepository.GetComplaintsByCategoryAsync();
    }

    public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(int monthsCount)
    {
        _logger.LogInformation("Getting monthly trend analytics for last {MonthsCount} months", monthsCount);
        return await _analyticsRepository.GetMonthlyTrendAsync(monthsCount);
    }
}