using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechSupport.Contracts.Requests;
using TechSupport.Contracts.Responses;
using TechSupport.Database;
using TechSupport.Database.Entities;
using TechSupport.Services;

namespace TechSupport.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;

    public IdentityController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Register(SignUpRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            UserName = request.Email,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) 
            return BadRequest(result.Errors);
        
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email, roles, user.FirstName, user.LastName);
        return Ok(new { access_token = token });
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Login(SignInRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "Неверный логин или пароль" });
        
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return BadRequest(new { Message = "Неверный логин или пароль" });
        
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email, roles, user.FirstName, user.LastName);
        return Ok(new { access_token = token });
    }
    
    [HttpGet("[action]"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var userInfo = new UserInformationResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            IsSupport = await _userManager.IsInRoleAsync(user, RolesEnum.Support)
        };
        return Ok(userInfo);
    }

    [HttpGet("get-support-role"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetSupportRoleAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return BadRequest();
        
        await _userManager.AddToRoleAsync(user, RolesEnum.Support);
        return Ok(await _userManager.GetRolesAsync(user));
    }
}