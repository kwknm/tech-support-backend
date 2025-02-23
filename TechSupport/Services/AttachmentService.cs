using TechSupport.Database;
using TechSupport.Database.Entities;

namespace TechSupport.Services;

public class AttachmentService : IAttachmentService
{
    private readonly ApplicationDbContext _context;

    public AttachmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> UploadAttachmentAsync(IFormFile attachment)
    {
        var fileExtension = Path.GetExtension(attachment.FileName).ToLower();

        using var memoryStream = new MemoryStream();
        await attachment.CopyToAsync(memoryStream);
        var data = memoryStream.ToArray();

        var newAttachment = new Attachment
        {
            FileName = attachment.FileName,
            FileExtension = fileExtension,
            Bytes = data,
            ContentType = attachment.ContentType
        };

        var entry = _context.Attachments.Add(newAttachment);
        await _context.SaveChangesAsync();

        return entry.Entity.Id;
    }
}