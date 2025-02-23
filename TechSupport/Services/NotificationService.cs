using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TechSupport.Database;
using TechSupport.Database.Entities;
using TechSupport.Hubs;

namespace TechSupport.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationHub> _notificationHub;
    private readonly ApplicationDbContext _context;

    public NotificationService(IHubContext<NotificationHub, INotificationHub> notificationHub, ApplicationDbContext context)
    {
        _notificationHub = notificationHub;
        _context = context;
    }

    public async Task SendNotificationAsync(string userId, string type, string title, string body, Metadata metadata )
    {
        var notification = new Notification
        {
            RecipientId = userId,
            IsRead = false,
            Body = body,
            Type = type,
            Title = title,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = metadata
        };

        var createdEntry = await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        await _notificationHub.Clients.User(userId)
            .ReceiveNotification(createdEntry.Entity.Id, createdEntry.Entity.IsRead,
                createdEntry.Entity.RecipientId, type, title, body,
                DateTimeOffset.UtcNow,
                metadata);
    }

    public async Task<IList<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _context.Notifications.Where(x => x.RecipientId == userId)
            .Include(x => x.Metadata).ToListAsync();
    }

    public async Task ReadAllNotificationsAsync(string userId)
    {
        await _context.Notifications.Where(x => x.RecipientId == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.IsRead, true));
    }
}