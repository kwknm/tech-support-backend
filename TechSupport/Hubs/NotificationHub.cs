using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TechSupport.Database.Entities;
using TechSupport.Services;

namespace TechSupport.Hubs;

public interface INotificationHub
{
    public Task ReceiveNotification(Guid id, bool isRead, string recipientId, string type, string title, string body,
        DateTimeOffset timestamp, Metadata metadata);
}

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationHub : Hub<INotificationHub>
{
    private readonly IChatService _chatService;

    public NotificationHub(IChatService chatService)
    {
        _chatService = chatService;
    }
}