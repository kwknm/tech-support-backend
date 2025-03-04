using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSupport.Database;
using TechSupport.Services;

namespace TechSupport.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AttachmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private IAttachmentService _attachmentService;

    public AttachmentsController(ApplicationDbContext context, IAttachmentService attachmentService)
    {
        _context = context;
        _attachmentService = attachmentService;
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> DownloadAttachmentAsync(Guid id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
            
        if (attachment is null)
            return NotFound();

        return File(attachment.Bytes, attachment.ContentType, attachment.FileName);
    }

    [HttpPost, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UploadAttachmentAsync([FromForm] IFormFile? attachment)
    {
        if (attachment is null)
            return BadRequest();
        
        if (attachment.Length > 10 * 1024 * 1024)
            return BadRequest(new { Message = "Размер вложения не может быть больше 10 Мб" });
        
        var attachmentId = await _attachmentService.UploadAttachmentAsync(attachment);

        return Ok(new { AttachmentId = attachmentId });
    }
}