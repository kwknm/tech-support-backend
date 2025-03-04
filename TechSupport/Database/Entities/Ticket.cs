using System.ComponentModel.DataAnnotations.Schema;

namespace TechSupport.Database.Entities;

public enum TicketStatus
{
    Registered,
    InProgress,
    Completed,
    Cancelled
}

public class Ticket
{
    public int Id { get; set; }
    public string IssuerId { get; set; }
    public User Issuer { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TicketStatus Status { get; set; }
    public Guid IssueTypeId { get; set; }
    public IssueType IssueType { get; set; }
    public string? SupportId { get; set; }
    public User? Support { get; set; }
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }
    [NotMapped] public bool IsClosed => Status is TicketStatus.Completed or TicketStatus.Cancelled;
}