namespace TechSupport.Contracts.Responses;

public record FaqResponse(
    int Id,
    string Title,
    string Content,
    UserResponse Author,
    IList<string> Likes,
    DateTimeOffset? EditedAt,
    DateTimeOffset CreatedAt
);