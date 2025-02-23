using TechSupport.Database.Entities;

namespace TechSupport.Contracts.Responses;

public record TicketResponse(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    string IssuerId,
    UserResponse Issuer,
    string? SupportId,
    UserResponse Support,
    Guid ChatId,
    DateTimeOffset CreatedAt,
    Guid? AttachmentId,
    AttachmentResponse? Attachment,
    bool IsClosed,
    IssueType IssueType
);