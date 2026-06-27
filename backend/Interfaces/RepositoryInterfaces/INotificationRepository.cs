using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetUserNotificationsAsync(int userId, int pageNumber, int pageSize);
    Task<int> GetUnreadCountAsync(int userId);
    Task<int> GetTotalCountAsync(int userId);
    Task<Notification?> GetByIdAsync(int id);
    Task UpdateAsync(Notification notification);
    Task AddAsync(Notification notification);
}
