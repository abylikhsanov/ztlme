using FastEndpoints;
using ztlme.Dtos;
using ztlme.Services;

namespace ztlme.Endpoints;

public class RegisterUserSuccessEndpoint : Endpoint<RegisterUserReq>
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterUserSuccessEndpoint> _logger;
    
    private string SetRedirectUrl(bool isProduction, bool isResponseOk)
    {
        if (isProduction)
        {
            if (isResponseOk)
            {
                return $"{Environment.GetEnvironmentVariable("FRONTEND_URI")!}{Environment.GetEnvironmentVariable("FRONTEND_URI_AUTH_SUCCESS")!}";
            }
            return Environment.GetEnvironmentVariable("FRONTEND_URI_AUTH_SUCCESS")!;
        }
        if (isResponseOk)
        {
            return _configuration["Frontend:AuthSuccessURI"]!;
        }
        return _configuration["Frontend:AuthSuccessURI"]!;
    }

    public RegisterUserSuccessEndpoint(IAuthService authService, IConfiguration configuration, ILogger<RegisterUserSuccessEndpoint> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/auth/success");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterUserReq req, CancellationToken ct)
    {
        var url = SetRedirectUrl(false, true);
        //_logger.LogInformation($"sessions: {req.SessionId}");
        HttpContext.Response.Redirect($"{url}");
        
    }
}