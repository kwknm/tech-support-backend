namespace TechSupport.Contracts.Requests;

public record CreateTicketRequest(string Title, string Description, Guid IssueTypeId, IFormFile? Attachment);