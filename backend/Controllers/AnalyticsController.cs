using ComplaintManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AnalyticsController
    : ControllerBase
{
    private readonly
        IAnalyticsService
        _analyticsService;

    public AnalyticsController(
        IAnalyticsService
            analyticsService)
    {
        _analyticsService =
            analyticsService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("dashboard")]
    public async Task<IActionResult>
        GetDashboard()
    {
        var response =
            await _analyticsService
                .GetDashboardSummaryAsync();

        return Ok(response);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("complaints-by-status")]
    public async Task<IActionResult>
        GetComplaintsByStatus()
    {
        var result =
            await _analyticsService
                .GetComplaintsByStatusAsync();

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("complaints-by-category")]
    public async Task<IActionResult> GetComplaintsByCategory()
    {
        var result = await _analyticsService.GetComplaintsByCategoryAsync();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("monthly-trend")]
    public async Task<IActionResult> GetMonthlyTrend([FromQuery] int monthsCount)
    {
        var result = await _analyticsService.GetMonthlyTrendAsync(monthsCount);
        return Ok(result);
    }
}