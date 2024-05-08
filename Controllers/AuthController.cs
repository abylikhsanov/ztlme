using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ztlme.Services;
using ztlme.Models;

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

    // Not used.
    [HttpGet("isAuth")]
    public ActionResult<bool> GetIsAuth()
    {
        var response = _authService.CheckAuth();
        return Ok(response);
    }

    // Do not use, expensive
    [HttpGet("bankid")]
    public IActionResult Authenticate()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/api/Auth/success"
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }
    
    // Do not use, expensive, requires bankid
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

    // If empty body, return false.
    // Before calling credit check API, check the DB
    // if (person in DB) -> If (CreditScoreOK) -> true else false
    [HttpPost("signup")]
    public async Task<ActionResult<bool>> SignUp([FromBody] PersonSignUp person)
    {
        // Check for credit and is lower than we need, reject (return false).
        // If ok, add to the database
        Console.WriteLine("signup");
        var response = await _authService.CheckIfCanBeSignedUp(person);
        return Ok(response);
    }
    
    
    
}