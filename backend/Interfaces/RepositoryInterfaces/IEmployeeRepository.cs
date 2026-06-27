using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface IEmployeeRepository
{   
    public Task<Employee> AddAsync(Employee employee);
    
    public Task<Employee?> GetByEmailAsync(string email);
    
    Task<List<Employee>>
        GetLeastLoadedEmployeesAsync(int departmentId);
    
    Task<Employee?> GetByIdAsync(
        int employeeId);
    
    Task<Employee?> GetTeamLeadByDepartmentAsync(
        int departmentId);

    Task<Employee?> GetManagerAsync();

    Task<Employee?> GetSeniorManagerAsync();
    
    Task<int> GetEmployeeCountAsync();
}