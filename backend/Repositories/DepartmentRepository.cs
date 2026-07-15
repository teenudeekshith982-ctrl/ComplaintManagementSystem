using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ComplaintManagementSystem.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ComplaintManagementSystemContext _context;
    public DepartmentRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(int departmentId)
    {
        return await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }

    public async Task<Department> AddAsync(Department department)
    {
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Department department)
    {
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
    }
}