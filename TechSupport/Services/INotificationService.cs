using TechSupport.Database.Entities;

namespace TechSupport.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string type, string title, string body, Metadata metadata);
    Task ReadAllNotificationsAsync(string userId);
    Task<IList<Notification>> GetUserNotificationsAsync(string userId);
}