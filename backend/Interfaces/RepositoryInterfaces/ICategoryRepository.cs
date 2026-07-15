using System.Threading.Tasks;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(int categoryId);
    Task<ComplaintCategory?> GetByIdAsync(int categoryId);
    Task<ComplaintCategory> AddAsync(ComplaintCategory category);
    Task UpdateAsync(ComplaintCategory category);
    Task DeleteAsync(ComplaintCategory category);
}