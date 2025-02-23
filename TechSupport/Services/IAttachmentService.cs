namespace TechSupport.Services;

public interface IAttachmentService
{
    Task<Guid> UploadAttachmentAsync(IFormFile attachment);
}