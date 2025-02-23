namespace TechSupport.Database.Entities;

public class Chat
{
    public Guid Id { get; set; }
    public string IssuerId { get; set; }
    public string? SupportId { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
}