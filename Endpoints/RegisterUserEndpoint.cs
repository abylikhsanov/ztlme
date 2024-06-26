using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using ztlme.Services;
using IAuthenticationService = Signicat.Express.Authentication.IAuthenticationService;

namespace ztlme.Endpoints;

public class RegisterUserEndpoint : EndpointWithoutRequest
{
    private readonly IAuthService _authService;
    private readonly string _frontendAppUrl;
    private readonly string _backendUrl;
    
    public RegisterUserEndpoint(IAuthService authService, 
        IConfiguration configuration)
    {
        _authService = authService;
        _frontendAppUrl = configuration["FrontendURI"]!;
        _backendUrl = configuration["BackendURI"]!;
    }
    
    public override void Configure()
    {
        Get("/api/user/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var request = HttpContext.Request;
        var redirectUri = $"{request.Scheme}://{request.Host}/api/auth/success";
        Console.WriteLine($"URI: {redirectUri}");

        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri // Ensure this is an absolute URI
        };

        Console.WriteLine("Initiating Challenge");
        await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);

        // Optionally set status code and complete response
        await HttpContext.Response.CompleteAsync();
    }
}