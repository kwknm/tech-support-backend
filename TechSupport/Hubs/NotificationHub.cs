using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TechSupport.Contracts.Responses;
using TechSupport.Database.Entities;
using TechSupport.Services;

namespace TechSupport.Hubs;

public interface INotificationHub
{
    public Task ReceiveNotification(ReceiveNotificationResponse response);
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