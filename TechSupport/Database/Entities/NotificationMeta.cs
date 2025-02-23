namespace TechSupport.Database.Entities;

public class Metadata
{
    public Guid Id { get; set; }
    public string? ResourceId { get; init; }
    public Guid NotificationId { get; init; }
}