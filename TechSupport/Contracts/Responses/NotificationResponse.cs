namespace TechSupport.Contracts.Responses;

public record MetadataResponse(string ResourceId);

public record NotificationResponse(
    string Id,
    string Type,
    string Title,
    string Body,
    string RecipientId,
    bool IsRead,
    MetadataResponse Metadata,
    Guid? AttachmentId,
    AttachmentResponse? Attachment,
    DateTimeOffset Timestamp);