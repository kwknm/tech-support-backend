using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TechSupport.Contracts.Responses;
using TechSupport.Database;
using TechSupport.Database.Entities;
using TechSupport.Services;

namespace TechSupport.Hubs;

public interface IChatHub
{
    public Task ReceiveMessage(string firstName, string lastName, string content, bool isSupport, DateTime timestamp,
        string userId, Attachment? attachment);

    public Task ReceiveSystemMessage(string content);
}

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub : Hub<IChatHub>
{
    private readonly IChatService _chatService;
    private readonly ApplicationDbContext _context;

    public ChatHub(IChatService chatService, ApplicationDbContext context)
    {
        _chatService = chatService;
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.ReceiveSystemMessage("con_id:" + Context.ConnectionId);
    }

    public async Task JoinChat(string chatId)
    {
        if (!await _chatService.IsAllowedToJoinChatAsync(Context.UserIdentifier!, chatId)) return;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        await Clients.Caller.ReceiveSystemMessage("join chat: " + chatId);
    }

    public async Task SendMessage(string chatId, string content, Guid? attachmentId)
    {
        var firstName = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value!;
        var lastName = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value!;
        var isSupport = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value == "Support";

        var attachment = await _context.Attachments.FindAsync(attachmentId);

        await Clients.Group(chatId).ReceiveMessage(firstName, lastName, content.Trim(), isSupport, DateTime.UtcNow,
            Context.UserIdentifier!, attachment);
        await _chatService.SaveMessageAsync(Context.UserIdentifier!, Guid.Parse(chatId), content.Trim(), isSupport,
            attachmentId);
    }
}