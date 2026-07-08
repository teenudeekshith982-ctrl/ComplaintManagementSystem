using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;

using ComplaintManagementSystem.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ComplaintManagementSystem.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        ILogger<NotificationService> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<(List<NotificationDto> Notifications, int TotalCount, int UnreadCount)> GetMyNotificationsAsync(int pageNumber, int pageSize)
    {
        var userId = _currentUserService.GetUserId();
        _logger.LogInformation("Fetching notifications for user {UserId}", userId);

        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, pageNumber, pageSize);
        var totalCount = await _notificationRepository.GetTotalCountAsync(userId);
        var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

        var notificationDtos = notifications.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            RelatedComplaintId = n.RelatedComplaintId,
            RelatedComplaintTitle = n.RelatedComplaint?.Title
        }).ToList();

        return (notificationDtos, totalCount, unreadCount);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var userId = _currentUserService.GetUserId();
        var notification = await _notificationRepository.GetByIdAsync(notificationId);

        if (notification == null)
        {
            throw new NotFoundException("Notification not found");
        }

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("You cannot modify this notification");
        }

        if (notification.IsRead)
        {
            _logger.LogInformation("Notification {NotificationId} was already marked as read", notificationId);
            return;
        }

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification);
        _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = _currentUserService.GetUserId();
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, 1, int.MaxValue);

        foreach (var notification in notifications.Where(n => !n.IsRead))
        {
            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }

        _logger.LogInformation("All notifications marked as read for user {UserId}", userId);
    }

    public async Task CreateAsync(int userId, string message, int? relatedComplaintId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedComplaintId = relatedComplaintId
        };

        await _notificationRepository.AddAsync(notification);
        _logger.LogInformation("Notification created for user {UserId}: {Message}", userId, message);

        try
        {
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
            {
                notificationId = notification.NotificationId,
                message = notification.Message,
                isRead = false,
                createdAt = notification.CreatedAt,
                relatedComplaintId = notification.RelatedComplaintId
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification to user {UserId}", userId);
        }
    }
}
