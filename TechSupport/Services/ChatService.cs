using Microsoft.EntityFrameworkCore;
using TechSupport.Database;
using TechSupport.Database.Entities;

namespace TechSupport.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly INotificationService _notificationService;

    public ChatService(ApplicationDbContext context, IEncryptionService encryptionService,
        INotificationService notificationService)
    {
        _context = context;
        _encryptionService = encryptionService;
        _notificationService = notificationService;
    }

    public async Task SaveMessageAsync(string userId, Guid chatId, string message, bool isSupport, Guid? attachmentId)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.ChatId == chatId);
        if (ticket is null || ticket.IsClosed)
            return;

        if (isSupport && ticket.SupportId != userId)
        {
            return;
        }

        var encryptedMessage = _encryptionService.AesEncrypt(message);

        var newMessage = new Message
        {
            ChatId = chatId,
            UserId = userId,
            IsSupport = isSupport,
            Content = encryptedMessage,
            Timestamp = DateTimeOffset.UtcNow,
            AttachmentId = attachmentId
        };

        var chat = await _context.Chats.FindAsync(chatId);

        if (chat is null)
        {
            return;
        }

        var companion = (userId == chat.IssuerId ? chat.SupportId : chat.IssuerId)!;
        await _notificationService.SendNotificationAsync(companion, NotificationTypeEnum.NewMessage,
            $"Вы получили ответ в чате заявки #{ticket.Id}",
            $"Текст сообщения: {message}",
            new Metadata
            {
                ResourceId = ticket.Id.ToString()
            });

        await _context.Messages.AddAsync(newMessage);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> IsAllowedToJoinChatAsync(string userId, string chatId)
    {
        var user = await _context.Users.FindAsync(userId);
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.ChatId == Guid.Parse(chatId));
        if (ticket is null || ticket.IsClosed)
            return false;

        return ticket.IssuerId == user!.Id || ticket.SupportId == user.Id;
    }
}