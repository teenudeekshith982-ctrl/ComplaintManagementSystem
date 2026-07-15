using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComplaintManagementSystem.Repositories
{
    public class ComplaintPriorityRepository : IComplaintPriorityRepository
    {
        private readonly ComplaintManagementSystemContext _context;

        public ComplaintPriorityRepository(ComplaintManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int priorityId)
        {
            return await _context.ComplaintPriorities.AnyAsync(p => p.PriorityId == priorityId);
        }

        public async Task<ComplaintPriority?> GetByIdAsync(int priorityId)
        {
            return await _context.ComplaintPriorities.FindAsync(priorityId);
        }

        public async Task<IEnumerable<ComplaintPriority>> GetAllAsync()
        {
            return await _context.ComplaintPriorities.ToListAsync();
        }

        public async Task<ComplaintPriority> AddAsync(ComplaintPriority priority)
        {
            await _context.ComplaintPriorities.AddAsync(priority);
            await _context.SaveChangesAsync();
            return priority;
        }

        public async Task UpdateAsync(ComplaintPriority priority)
        {
            _context.ComplaintPriorities.Update(priority);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ComplaintPriority priority)
        {
            _context.ComplaintPriorities.Remove(priority);
            await _context.SaveChangesAsync();
        }
    }
}
