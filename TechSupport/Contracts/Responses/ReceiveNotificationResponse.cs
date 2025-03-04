using TechSupport.Database.Entities;

namespace TechSupport.Contracts.Responses;

public record ReceiveNotificationResponse(Guid Id, bool IsRead, string RecipientId, string Type, string Title, string Body,
    DateTimeOffset Timestamp, Metadata Metadata);