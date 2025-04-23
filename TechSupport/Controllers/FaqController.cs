using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechSupport.Contracts.Requests;
using TechSupport.Database;
using TechSupport.Database.Entities;

namespace TechSupport.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FaqController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FaqController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetFaqsAsync([FromQuery] string? search)
    {
        // todo mapper all outputs
        var faqs = search is null
            ? await _context.Faqs.AsNoTracking().ToListAsync()
            : await _context.Faqs.AsNoTracking().Where(x => 
                x.Title.ToLower().Contains(search.ToLower())).ToListAsync();

        return Ok(faqs);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFaqByIdAsync(int id)
    {
        var faq = await _context.Faqs.FindAsync(id);
        return faq is not null ? Ok(faq) : NotFound(new { Message = "Запись не найдена" });
    }

    [HttpPost, Authorize(Roles = RolesEnum.Support, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateFaqAsync(CreateFaqRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        var faq = new Faq
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var entry = await _context.Faqs.AddAsync(faq);
        await _context.SaveChangesAsync();

        return Ok(new { entry.Entity.Id });
    }
}