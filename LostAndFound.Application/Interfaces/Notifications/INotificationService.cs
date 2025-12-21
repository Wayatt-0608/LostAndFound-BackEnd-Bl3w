using LostAndFound.Application.DTOs.Notifications;

namespace LostAndFound.Application.Interfaces.Notifications;

public interface INotificationService
{
    Task CreateNotificationAsync(
        int userId, 
        string title, 
        string message, 
        string type, 
        int? relatedEntityId = null, 
        string? relatedEntityType = null);
    
    Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(
        int userId, 
        bool? unreadOnly = null);
    
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    
    Task<int> GetUnreadCountAsync(int userId);
}

