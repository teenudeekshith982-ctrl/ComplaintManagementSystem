using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ComplaintManagementSystemContext _context;
    public AnalyticsRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }
    
    public async Task<DashboardSummaryDto>
    GetDashboardSummaryAsync()
{
    var totalComplaints =
        await _context.Complaints.CountAsync();

    var openComplaints =
        await _context.Complaints
            .CountAsync(c =>
                c.StatusId ==
                (int)ComplaintStatusEnum.Open);

    var resolvedComplaints =
        await _context.Complaints
            .CountAsync(c =>
                c.StatusId ==
                (int)ComplaintStatusEnum.Resolved);

    var closedComplaints =
        await _context.Complaints
            .CountAsync(c =>
                c.StatusId ==
                (int)ComplaintStatusEnum.Closed);

    var resolvedComplaintList =
        await _context.Complaints
            .Where(c =>
                c.ResolvedAt != null)
            .ToListAsync();

    double averageResolutionHours = 0;

    if (resolvedComplaintList.Any())
    {
        averageResolutionHours =
            resolvedComplaintList
                .Average(c =>
                    (c.ResolvedAt!.Value -
                     c.CreatedAt)
                    .TotalHours);
    }

    var breachedComplaints =
        await _context.Complaints
            .CountAsync(c =>
                c.DueDate != null
                &&
                c.ResolvedAt != null
                &&
                c.ResolvedAt >
                c.DueDate);

    double slaBreachRate = 0;

    if (totalComplaints > 0)
    {
        slaBreachRate =
            (double)breachedComplaints
            / totalComplaints * 100;
    }

    return new DashboardSummaryDto
    {
        TotalComplaints =
            totalComplaints,

        OpenComplaints =
            openComplaints,

        ResolvedComplaints =
            resolvedComplaints,

        ClosedComplaints =
            closedComplaints,

        AverageResolutionTimeHours =
            Math.Round(
                averageResolutionHours,
                2),

        SlaBreachRate =
            Math.Round(
                slaBreachRate,
                2)
    };
}
    public async Task<List<ComplaintStatusAnalyticsDto>>
        GetComplaintsByStatusAsync()
    {
        return await _context.Complaints
            .GroupBy(c => c.StatusId)
            .Select(g => new ComplaintStatusAnalyticsDto
            {
                Status =
                    ((ComplaintStatusEnum)g.Key)
                    .ToString(),

                Count =
                    g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<ComplaintCategoryAnalyticsDto>> GetComplaintsByCategoryAsync()
    {
        return await _context.Complaints
            .Include(c => c.ComplaintCategory)
            .GroupBy(c => c.CategoryId)
            .Select(g => new ComplaintCategoryAnalyticsDto
            {
                Category = g.Select(x => x.ComplaintCategory.Categoryname).FirstOrDefault() ?? "Unknown",
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(int monthsCount)
    {
        var result = new List<MonthlyTrendDto>();
        var today = DateTime.UtcNow;

        for (int i = monthsCount - 1; i >= 0; i--)
        {
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);

            var submittedCount = await _context.Complaints
                .CountAsync(c => c.CreatedAt >= firstDayOfMonth && c.CreatedAt <= lastDayOfMonth);

            var resolvedCount = await _context.Complaints
                .CountAsync(c => c.ResolvedAt.HasValue && c.ResolvedAt.Value >= firstDayOfMonth && c.ResolvedAt.Value <= lastDayOfMonth);

            result.Add(new MonthlyTrendDto
            {
                MonthName = firstDayOfMonth.ToString("MMMM yyyy"),
                SubmittedCount = submittedCount,
                ResolvedCount = resolvedCount
            });
        }

        return result;
    }
}