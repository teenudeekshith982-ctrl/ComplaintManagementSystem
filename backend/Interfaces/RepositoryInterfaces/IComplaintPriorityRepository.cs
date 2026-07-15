using System.Collections.Generic;
using System.Threading.Tasks;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces
{
    public interface IComplaintPriorityRepository
    {
        Task<bool> ExistsAsync(int priorityId);
        Task<ComplaintPriority?> GetByIdAsync(int priorityId);
        Task<IEnumerable<ComplaintPriority>> GetAllAsync();
        Task<ComplaintPriority> AddAsync(ComplaintPriority priority);
        Task UpdateAsync(ComplaintPriority priority);
        Task DeleteAsync(ComplaintPriority priority);
    }
}
