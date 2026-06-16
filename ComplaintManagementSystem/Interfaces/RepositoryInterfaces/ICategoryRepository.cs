namespace ComplaintManagementSystem.Interfaces;

public interface ICategoryRepository
{
    public Task<bool> ExistsAsync(int categoryId);
}