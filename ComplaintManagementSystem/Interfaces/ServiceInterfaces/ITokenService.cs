using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface ITokenService
{
    public Task<string> CreateToken(User user);
}