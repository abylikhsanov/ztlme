using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ztlme.Models;
using ztlme.Services;

namespace ztlme.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("bankid")]
    public IActionResult Authenticate()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/api/Auth/success"
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }
    
    [HttpGet("success")]
    public async Task<ActionResult<string>> Get()
    {
        Console.WriteLine("AAAAA");
        return Ok(await _authService.AuthSuccessBankId());
    }
    
}