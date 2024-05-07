using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using ztlme.Services;

namespace ztlme.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpGet("isAuth")]
    public ActionResult<bool> GetIsAuth()
    {
        var response = _authService.CheckAuth();
        return Ok(response);
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
        Console.WriteLine("success, redirecting");
        var response = await _authService.AuthSuccessBankId();
        if (response.Success)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return Redirect($"{Environment.GetEnvironmentVariable("FRONTEND_URI")!}/{Environment.GetEnvironmentVariable("FRONTEND_URI_AUTH_SUCCESS")!}");
            }
            return Redirect(_configuration["Frontend:AuthSuccessURI"]!);
        }
        return Ok(await _authService.AuthSuccessBankId());
    }
    
}