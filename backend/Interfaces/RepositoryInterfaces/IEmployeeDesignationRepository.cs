using System.Collections.Generic;
using System.Threading.Tasks;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces
{
    public interface IEmployeeDesignationRepository
    {
        Task<bool> ExistsAsync(int designationId);
        Task<EmployeeDesignation?> GetByIdAsync(int designationId);
        Task<IEnumerable<EmployeeDesignation>> GetAllAsync();
        Task<EmployeeDesignation> AddAsync(EmployeeDesignation designation);
        Task UpdateAsync(EmployeeDesignation designation);
        Task DeleteAsync(EmployeeDesignation designation);
    }
}
