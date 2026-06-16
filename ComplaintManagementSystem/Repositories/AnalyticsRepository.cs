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
    

}