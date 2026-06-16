using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class EscalationRepository : IEscalationRepository
{   
    private readonly ComplaintManagementSystemContext _context;
    public EscalationRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }
    
    public async Task<EscalatedComplaint?>
        GetLatestEscalationAsync(
            int complaintId)
    {
        return await _context.EscalatedComplaints
            .Where(e => e.ComplaintId == complaintId)
            .OrderByDescending(e => e.EscalatedAt)
            .FirstOrDefaultAsync();
    }
    
    public async Task<EscalatedComplaint> AddAsync(
        EscalatedComplaint escalation)
    {
        await _context.EscalatedComplaints
            .AddAsync(escalation);

        await _context.SaveChangesAsync();

        return escalation;
    }
    
    public async Task<(List<EscalatedComplaint>, int)>
        GetEscalationsAsync(
            EscalationFilterDto filter)
    {
        var query =
            _context.EscalatedComplaints
                .Include(e => e.EscalatedLevel)
                .Include(e => e.Complaint)
                .ThenInclude(c => c.Employee)
                .ThenInclude(e => e.User)
                .Include(e => e.Complaint)
                .ThenInclude(c => c.Employee)
                .ThenInclude(e => e.Department)
                .AsQueryable();

        if (filter.EscalationLevelId.HasValue)
        {
            query = query.Where(e =>
                e.EscalatedLevelId ==
                filter.EscalationLevelId.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(e =>
                e.Complaint!.Employee!.DepartmentId ==
                filter.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(
                filter.SearchTerm))
        {
            query = query.Where(e =>
                e.Reason.Contains(
                    filter.SearchTerm)

                ||

                e.Complaint!.Title.Contains(
                    filter.SearchTerm));
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(e =>
                e.EscalatedAt >=
                filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(e =>
                e.EscalatedAt <=
                filter.ToDate.Value);
        }

        var totalRecords =
            await query.CountAsync();

        var escalations =
            await query
                .OrderByDescending(e =>
                    e.EscalatedAt)
                .Skip(
                    (filter.PageNumber - 1)
                    * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

        return (
            escalations,
            totalRecords);
    }
    
    public async Task<int>
        GetEscalationCountAsync()
    {
        return await _context.EscalatedComplaints
            .CountAsync();
    }
}