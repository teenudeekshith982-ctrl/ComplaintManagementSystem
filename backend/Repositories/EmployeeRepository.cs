using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ComplaintManagementSystemContext _context;

    public EmployeeRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }

    public async Task<Employee> AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e =>
                e.User!.Email == email);
    }
    
    public async Task<List<Employee>> GetLeastLoadedEmployeesAsync(int  departmentId)
    {
        var employeeLoads = await _context.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive && e.DepartmentId == departmentId && e.DesignationId == (int)EmployeeDesignationEnum.Employee)
            .Select(e => new
            {
                Employee = e,

                Load = _context.Complaints
                    .Where(c => c.EmployeeId == e.EmployeeId && 
                                c.StatusId != (int)ComplaintStatusEnum.Resolved && 
                                c.StatusId != (int)ComplaintStatusEnum.Closed)
                    .Sum(c => c.PriorityId == (int)ComplaintPriorityEnum.Critical ? 4 :
                              c.PriorityId == (int)ComplaintPriorityEnum.High ? 3 :
                              c.PriorityId == (int)ComplaintPriorityEnum.Medium ? 2 : 1)
            })
            .ToListAsync();

        if (!employeeLoads.Any())
        {
            return new List<Employee>();
        }

        var minimumLoad = employeeLoads.Min(x => x.Load);

        return employeeLoads
            .Where(x => x.Load == minimumLoad)
            .Select(x => x.Employee)
            .ToList();
    }

    public Task<Employee?> GetByIdAsync(
        int employeeId)
    {
        return _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }
    
    
    public async Task<Employee?>
        GetTeamLeadByDepartmentAsync(
            int departmentId)
    {
        return await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e =>
                e.DepartmentId == departmentId
                &&
                e.IsActive
                &&
                e.DesignationId == (int)EmployeeDesignationEnum.TeamLead);
    }
    
    public async Task<Employee?>
        GetManagerAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e =>
                e.IsActive
                &&
                e.DesignationId == (int)EmployeeDesignationEnum.Manager);
    }
    
    public async Task<Employee?>
        GetSeniorManagerAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e =>
                e.IsActive
                &&
                e.DesignationId == (int)EmployeeDesignationEnum.SeniorManager);
    }
    
    public async Task<int>
        GetEmployeeCountAsync()
    {
        return await _context.Employees
            .CountAsync();
    }
}