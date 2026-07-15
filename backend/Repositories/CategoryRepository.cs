using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

    public async Task<ComplaintCategory?> GetByIdAsync(int categoryId)
    {
        return await _context.ComplaintCategories.FindAsync(categoryId);
    }

    public async Task<ComplaintCategory> AddAsync(ComplaintCategory category)
    {
        await _context.ComplaintCategories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(ComplaintCategory category)
    {
        _context.ComplaintCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ComplaintCategory category)
    {
        _context.ComplaintCategories.Remove(category);
        await _context.SaveChangesAsync();
    }
}