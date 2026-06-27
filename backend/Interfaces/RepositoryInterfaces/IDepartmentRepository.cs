using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface IDepartmentRepository
{
    public Task<Department?> GetByIdAsync(int departmentId);
}