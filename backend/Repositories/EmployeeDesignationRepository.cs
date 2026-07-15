using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComplaintManagementSystem.Repositories
{
    public class EmployeeDesignationRepository : IEmployeeDesignationRepository
    {
        private readonly ComplaintManagementSystemContext _context;

        public EmployeeDesignationRepository(ComplaintManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int designationId)
        {
            return await _context.EmployeeDesignations.AnyAsync(d => d.DesignationId == designationId);
        }

        public async Task<EmployeeDesignation?> GetByIdAsync(int designationId)
        {
            return await _context.EmployeeDesignations.FindAsync(designationId);
        }

        public async Task<IEnumerable<EmployeeDesignation>> GetAllAsync()
        {
            return await _context.EmployeeDesignations.ToListAsync();
        }

        public async Task<EmployeeDesignation> AddAsync(EmployeeDesignation designation)
        {
            await _context.EmployeeDesignations.AddAsync(designation);
            await _context.SaveChangesAsync();
            return designation;
        }

        public async Task UpdateAsync(EmployeeDesignation designation)
        {
            _context.EmployeeDesignations.Update(designation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(EmployeeDesignation designation)
        {
            _context.EmployeeDesignations.Remove(designation);
            await _context.SaveChangesAsync();
        }
    }
}
