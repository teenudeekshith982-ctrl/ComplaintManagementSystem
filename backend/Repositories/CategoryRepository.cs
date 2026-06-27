using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class CategoryRepository : ICategoryRepository
{
        private readonly ComplaintManagementSystemContext _context;

        public CategoryRepository(ComplaintManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int categoryId)
        {
            return await _context.ComplaintCategories
                .AnyAsync(c =>
                    c.CategoryId == categoryId);
        }
    
}