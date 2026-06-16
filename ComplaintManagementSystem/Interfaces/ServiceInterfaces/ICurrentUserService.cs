namespace ComplaintManagementSystem.Interfaces;

public interface ICurrentUserService
{
    public int GetUserId();
    public string GetRole();

    public int? GetEmployeeId();
    public string GetUserName();
}