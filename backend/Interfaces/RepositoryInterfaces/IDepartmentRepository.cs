using ComplaintManagementSystem.Models;
using System.Threading.Tasks;

namespace ComplaintManagementSystem.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int departmentId);
    Task<Department> AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(Department department);
}