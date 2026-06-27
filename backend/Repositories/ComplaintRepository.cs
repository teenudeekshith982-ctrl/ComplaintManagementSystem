using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class ComplaintRepository : IComplaintRepository
{   
    private readonly ComplaintManagementSystemContext _context;

    public ComplaintRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Complaint complaint)
    {
       await  _context.AddAsync(complaint);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Complaint> Complaints, int TotalRecords)>
        GetUserComplaintsAsync(
            int userId,
            GetMyComplaintRequestDto request)
    {
        var query = _context.Complaints.AsQueryable();
        
        query = query.Where(c=>c.UserId == userId);

        if (request.Status.HasValue)
        {
            query = query.Where(c=>c.StatusId == (int)request.Status.Value);
        }

        if (request.Category.HasValue)
        {
            query = query.Where(c=>c.CategoryId == (int)request.Category.Value);
        }
        
        if (request.FromDate.HasValue)
        {
            var fromDate = DateTime.SpecifyKind(
                request.FromDate.Value,
                DateTimeKind.Utc);

            query = query.Where(c => c.CreatedAt >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDate = DateTime.SpecifyKind(
                request.ToDate.Value,
                DateTimeKind.Utc);

            query = query.Where(c => c.CreatedAt <= toDate);
        }
        
        if (!string.IsNullOrWhiteSpace(
                request.SearchTerm))
        {
            query = query.Where(c =>
                c.Title.Contains(
                    request.SearchTerm));
        }

        var totalRecords = await query.CountAsync();
        
        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (complaints, totalRecords);
    }

    
    
    public async Task<(List<Complaint> Complaints, int TotalRecords)>
        GetComplaintsAsync(
            GetComplaintRequestDto request,
            int? employeeId = null)
    {
        var query = _context.Complaints
            .Include(c => c.User)
            .Include(c => c.Employee)
            .AsQueryable();
        
        if (employeeId.HasValue)
        {
            query = query.Where(
                c => c.EmployeeId == employeeId.Value);
        }
        
        if (request.Status.HasValue)
        {
            query = query.Where(
                c => c.StatusId == (int)request.Status.Value);
        }
        
        if (request.Priority.HasValue)
        {
            query = query.Where(
                c => c.PriorityId == (int)request.Priority.Value);
        }
        
        if (request.Category.HasValue)
        {
            query = query.Where(
                c => c.CategoryId == (int)request.Category.Value);
        }
        
        if (request.FromDate.HasValue)
        {
            query = query.Where(
                c => c.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(
                c => c.CreatedAt <= request.ToDate.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(
                request.SearchTerm))
        {
            query = query.Where(c =>
                c.Title.Contains(request.SearchTerm));
        }
        
        var totalRecords = await query.CountAsync();
        
        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
        return (complaints, totalRecords);
    }

    public async Task<Complaint?> GetByIdAsync(int complaintId)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Employee)
            .Include(c => c.User)
            .Include(c => c.Comments)
            .Include(c => c.ComplaintAttachments)
            .Include(c => c.ComplaintCategory)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

        return complaint;
    } 

    public async Task UpdateAsync(Complaint complaint)
    {
        _context.Complaints.Update(complaint);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Complaint>>
        GetSlaBreachedComplaintsAsync()
    {
        return await _context.Complaints
            .Where(c =>
                c.DueDate != null

                &&

                c.DueDate < DateTime.UtcNow

                &&

                c.StatusId !=
                (int)ComplaintStatusEnum.Resolved

                &&

                c.StatusId !=
                (int)ComplaintStatusEnum.Closed)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Complaints.CountAsync();
    }
    
    public async Task<int>
        GetCountByStatusAsync(int statusId)
    {
        return await _context.Complaints
            .CountAsync(c =>
                c.StatusId == statusId);
    }
    
    
    
}