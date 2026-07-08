using System.Security.Claims;
using ComplaintManagementSystem.Interfaces;

namespace ComplaintManagementSystem.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated or user ID claim is missing.");
        }
        return userId;
    }
    
    public string GetRole()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value 
               ?? string.Empty;
    }

    public int? GetEmployeeId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("EmployeeId");
        if (claim == null || !int.TryParse(claim.Value, out int employeeId))
        {
            return null;
        }
        return employeeId;
    }
    
    public string GetUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst("name")?.Value 
               ?? user?.FindFirst(ClaimTypes.Name)?.Value 
               ?? user?.Identity?.Name 
               ?? string.Empty;
    }
}