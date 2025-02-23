namespace TechSupport.Contracts.Responses;

public record AttachmentResponse(
    Guid Id,
    string FileName,
    string FileExtension,
    int BytesLength);