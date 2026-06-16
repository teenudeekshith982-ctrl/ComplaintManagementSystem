using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface IUserRepository
{
    public Task<User> AddAsync(User user);
    public Task<User> GetByEmailAsync(string requestEmail);
    public Task<User?> GetByPhoneAsync(string phone);
    
    public Task<User?> GetByIdAsync (int id);
}