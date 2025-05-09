using System.Security.Claims;
using FluentValidation;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechSupport.Contracts.Requests;
using TechSupport.Contracts.Responses;
using TechSupport.Database;
using TechSupport.Database.Entities;
using TechSupport.Services;

namespace TechSupport.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IAttachmentService _attachmentService;
    private readonly IChatService _chatService;

    public TicketsController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper,
        INotificationService notificationService,
        IAttachmentService attachmentService, IChatService chatService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _notificationService = notificationService;
        _attachmentService = attachmentService;
        _chatService = chatService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _context.Tickets
            .Include(x => x.IssueType)
            .Include(x => x.Support)
            .Include(x => x.Issuer)
            .Include(x => x.Attachment)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return result is not null ? Ok(_mapper.Map<TicketResponse>(result)) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketsAsync([FromQuery] string? userId, [FromQuery] int? statusId,
        [FromQuery] string? supportId)
    {
        var result = _context.Tickets
            .Include(x => x.IssueType)
            .Include(x => x.Issuer)
            .Include(x => x.Support)
            .OrderByDescending(x => x.Id)
            .AsNoTracking()
            .AsQueryable();

        if (userId is not null) result = result.Where(x => x.IssuerId == userId);

        if (statusId is not null) result = result.Where(x => x.Status == (TicketStatus)statusId);

        if (supportId is not null) result = result.Where(x => x.SupportId == supportId);

        return Ok(_mapper.Map<List<TicketResponse>>(await result.ToListAsync()));
    }

    [HttpGet("issues")]
    public async Task<IActionResult> GetIssueTypesAsync()
    {
        var issueTypes = await _context.IssueTypes.AsNoTracking().ToListAsync();
        return Ok(issueTypes);
    }

    [HttpPost("issues"),
     Authorize(Roles = RolesEnum.Support, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateIssueTypeAsync([FromBody] CreateIssueRequest request)
    {
        if (await _context.IssueTypes.AsNoTracking().AnyAsync(x => x.Name == request.Name))
        {
            return BadRequest(new { Message = "Такой тип проблемы уже есть в системе" });
        }

        var newIssueType = new IssueType { Name = request.Name };
        var entry = await _context.IssueTypes.AddAsync(newIssueType);
        await _context.SaveChangesAsync();
        return Created($"/api/tickets/issues", new { IssueId = entry.Entity.Id });
    }

    [HttpDelete("issues/{id:guid}"),
     Authorize(Roles = RolesEnum.Support, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteIssueTypeAsync(Guid id)
    {
        var issueType = await _context.IssueTypes.FindAsync(id);
        if (issueType is null)
        {
            return NoContent();
        }

        _context.IssueTypes.Remove(issueType);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateAsync([FromForm] CreateTicketRequest request,
        [FromServices] IValidator<CreateTicketRequest> validator)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            return BadRequest(new { Errors = result.ToDictionary().Values });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        var chat = (await _context.Chats.AddAsync(new Chat
        {
            IssuerId = userId
        })).Entity;
        await _context.SaveChangesAsync();

        var newTicket = new Ticket
        {
            Title = request.Title,
            Description = request.Description.Trim(),
            Status = TicketStatus.Registered,
            IssuerId = userId,
            Chat = chat,
            IssueTypeId = request.IssueTypeId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (request.Attachment is not null)
        {
            if (request.Attachment.Length > 10 * 1024 * 1024)
                return BadRequest(new { Message = "Размер вложения не может быть больше 10 Мб" });
            var attachmentId = await _attachmentService.UploadAttachmentAsync(request.Attachment);
            newTicket.AttachmentId = attachmentId;
        }

        var entry = await _context.Tickets.AddAsync(newTicket);
        await _context.SaveChangesAsync();

        return Created($"/api/tickets/${entry.Entity.Id}", new { TicketId = entry.Entity.Id });
    }

    [HttpPost("{id:int}/assign"),
     Authorize(Roles = RolesEnum.Support, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AssignTicketAsync(int id)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);
        if (ticket is null)
            return BadRequest(new { Message = "Заявка не найдена" });

        if (ticket.SupportId is not null)
            return BadRequest(new { Message = "Заявка уже занята сотрудником поддержки" });

        var support = await _userManager.GetUserAsync(User);
        var chat = await _context.Chats.FindAsync(ticket.ChatId);

        chat!.SupportId = support!.Id;
        ticket.Status = TicketStatus.InProgress;
        ticket.Support = support;
        await _context.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(ticket.IssuerId, NotificationTypeEnum.TicketStatusChange,
            $"Статус вашей заявки #{ticket.Id} изменился",
            "Ваша заявка была принята на рассмотрение", new Metadata { ResourceId = ticket.Id.ToString() });

        return Ok();
    }

    [HttpGet("{id:int}/messages"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMessagesAsync(int id)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (ticket is null)
            return BadRequest(new { Message = "Заявка не найдена" });

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (ticket.SupportId != currentUserId && ticket.IssuerId != currentUserId)
            return Forbid();

        var mapped = _mapper.Map<List<MessageResponse>>(await _chatService.GetMessagesAsync(ticket.ChatId));
        return Ok(mapped);
    }

    [HttpPost("{id:int}/close"),
     Authorize(Roles = RolesEnum.Support, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CloseTicketAsync(int id)
    {
        return await UpdateTicketStatusAsync(id, TicketStatus.Completed, "Ваша заявка была закрыта");
    }

    [HttpPost("{id:int}/reject"),
     Authorize(Roles = RolesEnum.Support, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RejectTicketAsync(int id)
    {
        return await UpdateTicketStatusAsync(id, TicketStatus.Cancelled, "Ваша заявка была отклонена");
    }

    private async Task<IActionResult> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus,
        string successMessage)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(x => x.Id == ticketId);
        if (ticket is null)
            return BadRequest(new { Message = "Заявка не найдена" });

        if (ticket.Status != TicketStatus.InProgress)
            return BadRequest(new { Message = "Заявка должна быть на рассмотрении перед изменением статуса" });

        if (ticket.SupportId != User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value)
            return BadRequest(new { Message = "Вы не рассматриваете эту заявку" });

        ticket.Status = newStatus;
        ticket.ClosedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(ticket.IssuerId, NotificationTypeEnum.TicketStatusChange,
            $"Статус вашей заявки #{ticket.Id} изменился",
            successMessage, new Metadata { ResourceId = ticket.Id.ToString() });

        return Ok();
    }

    [HttpPatch("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateTicketAsync(int id, UpdateTicketRequest request)
    {
        var userId = User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket is null)
        {
            return NotFound(new { Message = "Заявка не найдена" });
        }

        if (ticket.IssuerId != userId)
        {
            return Forbid();
        }

        if (request.Description is not null)
        {
            ticket.Description = request.Description.Trim();
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}