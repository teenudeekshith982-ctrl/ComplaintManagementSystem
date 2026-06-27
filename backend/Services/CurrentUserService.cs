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
        var userid = _httpContextAccessor.HttpContext?
            .User
            .FindFirst(ClaimTypes.NameIdentifier)
            .Value;
        return int.Parse(userid);
        
    }
    
    public string GetRole()
    {
        return _httpContextAccessor.HttpContext?
            .User
            .FindFirst(System.Security.Claims.ClaimTypes.Role)?
            .Value!;
    }

    public int? GetEmployeeId()
    {
        var employeeId = _httpContextAccessor.HttpContext?
            .User
            .FindFirst("EmployeeId")?
            .Value;

        if (string.IsNullOrEmpty(employeeId))
        {
            return null;
        }

        return int.Parse(employeeId);
    }
    
    public string GetUserName()
    {
        return _httpContextAccessor
                   .HttpContext?
                   .User
                   .Identity?
                   .Name
               ?? string.Empty;
    }
}