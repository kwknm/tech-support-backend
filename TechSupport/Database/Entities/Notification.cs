namespace TechSupport.Database.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string RecipientId { get; set; }
    public User Recipient { get; set; }
    public bool IsRead { get; set; }
    public Metadata Metadata { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}