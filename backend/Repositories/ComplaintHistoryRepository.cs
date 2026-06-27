using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class ComplaintHistoryRepository : IComplaintHistoryRepository
{
    private readonly ComplaintManagementSystemContext _context;

    public ComplaintHistoryRepository(
        ComplaintManagementSystemContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        ComplaintHistory complaintHistory)
    {
        await _context.ComplaintHistories
            .AddAsync(complaintHistory);
        await _context.SaveChangesAsync();
        
        
        
    }
    
    public async Task<List<ComplaintHistory>> GetComplaintHistoryByComplaintIdAsync(int complaintId)
    {
        return await _context.ComplaintHistories.Where(c=>c.ComplaintId == complaintId)
            .OrderBy(c=>c.CreatedAt)                                
            .ToListAsync();
    }
}