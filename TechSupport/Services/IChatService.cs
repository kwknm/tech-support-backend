using TechSupport.Database.Entities;

namespace TechSupport.Services;

public interface IChatService
{
    Task SaveMessageAsync(string userId, Guid chatId, string message, bool isSupport, Guid? attachmentId);
    Task<bool> IsAllowedToJoinChatAsync(string userId, string chatId);
    Task<IEnumerable<Message>> GetMessagesAsync(Guid chatId);
}