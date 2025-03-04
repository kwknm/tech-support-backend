using System.Security.Claims;
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
    public Task ReceiveMessage(ReceiveMessageResponse response);

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
        await Clients.Caller.ReceiveSystemMessage("connection_id: " + Context.ConnectionId);
    }

    public async Task JoinChat(string chatId)
    {
        if (!await _chatService.IsAllowedToJoinChatAsync(Context.UserIdentifier!, chatId)) return;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        await Clients.Caller.ReceiveSystemMessage("join_chat: " + chatId);
    }

    public async Task SendMessage(string chatId, string content, Guid? attachmentId)
    {
        var firstName = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value!;
        var lastName = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value!;
        var isSupport = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value == "Support";

        Attachment? attachment = null;
        if (attachmentId is not null)
            attachment = await _context.Attachments.FindAsync(attachmentId);
        
        var response = new ReceiveMessageResponse(firstName, lastName, content.Trim(), isSupport, DateTime.UtcNow,
            Context.UserIdentifier!, attachment);

        await Clients.Group(chatId).ReceiveMessage(response);
        await _chatService.SaveMessageAsync(Context.UserIdentifier!, Guid.Parse(chatId), content.Trim(), isSupport,
            attachmentId);
    }
}