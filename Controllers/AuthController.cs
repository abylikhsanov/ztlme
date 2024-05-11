using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ztlme.Dtos;
using ztlme.Services;
using ztlme.Models;

namespace ztlme.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
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
    public async Task<ActionResult<SignupUserResDto>> SignUp([FromBody] SignupUserReqDto req)
    {
        // Check for credit and is lower than we need, reject (return false).
        // If ok, add to the database
        _logger.LogInformation("Starting signup process...");
        try
        {
            _logger.LogInformation("Checking credit score...");
            var response = await _authService.CheckIfCanBeSignedUp(req.PersonalNumber);
            _logger.LogInformation("Credit check response: " + response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during signup: " + ex.ToString());
            return StatusCode(400, "Internal Server Error: " + ex.Message);
        }
    }
    
    
    
}