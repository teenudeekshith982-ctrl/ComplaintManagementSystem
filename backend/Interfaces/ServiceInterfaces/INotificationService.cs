using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface INotificationService
{
    Task<(List<NotificationDto> Notifications, int TotalCount, int UnreadCount)> GetMyNotificationsAsync(int pageNumber, int pageSize);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync();
    Task CreateAsync(int userId, string message, int? relatedComplaintId = null);
}
