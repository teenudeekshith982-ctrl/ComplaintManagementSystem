using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class SlaRepository : ISlaRepository
{   
    private readonly ComplaintManagementSystemContext _context;

    public SlaRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }
    
    public async Task<SLA?> GetByPriorityIdAsync(int priorityid)
    {
        return await _context.SLAs.FirstOrDefaultAsync(p => p.PriorityId == priorityid);    
    }
}