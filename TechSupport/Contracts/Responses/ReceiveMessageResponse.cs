using TechSupport.Database.Entities;

namespace TechSupport.Contracts.Responses;

public record ReceiveMessageResponse(string FirstName, string LastName, string Content, bool IsSupport, DateTime Timestamp,
    string UserId, Attachment? Attachment);