using TechSupport.Database.Entities;

namespace TechSupport.Contracts.Responses;

public record MessageResponse(Guid Id, UserResponse User, string Content, Guid ChatId, string UserId, bool IsSupport, DateTimeOffset Timestamp, Attachment? Attachment);