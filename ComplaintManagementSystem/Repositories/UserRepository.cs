using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ComplaintManagementSystemContext _context;
    
    public UserRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }
    
    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        return  await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }
}