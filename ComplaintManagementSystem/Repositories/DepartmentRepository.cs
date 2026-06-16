using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ComplaintManagementSystemContext _context;
    public DepartmentRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }

    public Task<Department?> GetByIdAsync(int departmentId)
    {
        return _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }
}