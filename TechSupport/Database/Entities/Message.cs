namespace TechSupport.Database.Entities;

public class Message
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public bool IsSupport { get; set; }
    public Guid? AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}