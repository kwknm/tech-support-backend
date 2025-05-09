using System.Security.Claims;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechSupport.Contracts.Requests;
using TechSupport.Contracts.Responses;
using TechSupport.Database;
using TechSupport.Database.Entities;

namespace TechSupport.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FaqController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FaqController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetFaqsAsync([FromQuery] string? search)
    {
        var faqs = search is null
            ? await _context.Faqs.AsNoTracking().Include(x => x.Author).OrderBy(x => x.Id).ToListAsync()
            : await _context.Faqs.AsNoTracking().Include(x => x.Author).OrderBy(x => x.Id).Where(x =>
                x.Title.ToLower().Contains(search.ToLower())).ToListAsync();

        return Ok(_mapper.Map<List<FaqResponse>>(faqs));
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

    [HttpPost("{id:int}/like/toggle"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ToggleLikeFaqAsync(int id)
    {
        var faq = await _context.Faqs.FindAsync(id);

        if (faq is null)
        {
            return NotFound(new { Message = "Запись не найдена" });
        }

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!faq.Likes.Remove(currentUserId))
        {
            faq.Likes.Add(currentUserId);
        }

        await _context.SaveChangesAsync();

        return Ok(new { Likes = faq.Likes.Count });
    }
}