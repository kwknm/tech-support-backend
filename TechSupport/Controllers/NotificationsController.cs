using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSupport.Contracts.Responses;
using TechSupport.Services;

namespace TechSupport.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public NotificationsController(IMapper mapper, INotificationService notificationService)
    {
        _mapper = mapper;
        _notificationService = notificationService;
    }

    [HttpGet, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetUserNotificationsAsync()
    {
        var userId = User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(_mapper.Map<IList<NotificationResponse>>(notifications.Reverse()));
    }
    
    [HttpPost("read"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ReadAllNotificationsAsync()
    {
        var userId = User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        await _notificationService.ReadAllNotificationsAsync(userId);
        return Ok();
    }
}