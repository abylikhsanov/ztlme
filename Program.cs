using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Criipto.Signatures;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.OpenApi.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Signicat.Express;
using Signicat.Express.Authentication;
using ztlme.Data;
using ztlme.Services;
using Environment = System.Environment;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole(options => {
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
builder.Services.AddDbContext<DataContext>(options =>
{
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
    {
        var m = Regex.Match(Environment.GetEnvironmentVariable("DATABASE_URL")!, @"postgres://(.*):(.*)@(.*):(.*)/(.*)");
        options.UseNpgsql($"Server={m.Groups[3]};Port={m.Groups[4]};User Id={m.Groups[1]};Password={m.Groups[2]};Database={m.Groups[5]};sslmode=Prefer;Trust Server Certificate=true");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("ztlme_db"));
    }
    
});
builder.Services.AddCors(options =>
{
    string? frontendUrl = builder.Configuration["Frontend:RootURI"];
    if (frontendUrl == null)
    {
        frontendUrl = "http://localhost:3000";
    }
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
    {
        frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URI")!;
    }
    

    options.AddPolicy("MyCorsPolicy", builder =>
        builder.WithOrigins(frontendUrl)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Important for cookies to be allowed
});

// Add FastEndpoints
builder.Services.AddFastEndpoints();


//Criipto Auth
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Signicat
/*builder.Services.AddAuthentication()
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = "SignicatAuth:ClientId";
        options.ClientSecret = "Client Secret provided by our broker";
        options.Authority = "https://ztlme.sandbox.signicat.com/auth/open"; //issuer from well-known configuration
        options.ResponseType = "code";
        options.GetClaimsFromUserInfoEndpoint = true;
        options.Scope.Clear();
        options.Scope.Add("openid");
        //The next section is only for extra parameters to be send if required
        options.Events = new OpenIdConnectEvents 
        {
            OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.SetParameter("parameter name", "parameter value");
                return Task.FromResult(0);
            }
        };
    });*/

builder.Services.AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options => {

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            options.ClientId = Environment.GetEnvironmentVariable("CRIIPTO_AUTH_CLIENT_ID");
            options.ClientSecret = Environment.GetEnvironmentVariable("CRIIPTO_AUTH_CLIENT_SECRET");
            options.Authority = $"https://{Environment.GetEnvironmentVariable("CRIIPTO_AUTH_DOMAIN")}/";
        }
        else
        {
            options.ClientId = builder.Configuration["CriiptoAuth:ClientId"];
            options.ClientSecret = builder.Configuration["CriiptoAuth:Secret"];
            options.Authority = $"https://{builder.Configuration["CriiptoAuth:Domain"]}/";
        }
        options.ResponseType = "code";
        options.SkipUnrecognizedRequests = true;

        // The next to settings must match the Callback URLs in Criipto Verify
        options.CallbackPath = new PathString("/api/Auth/success"); 
        options.SignedOutCallbackPath = new PathString("/api/auth/signout");

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace("http:", "https:");
                return Task.CompletedTask;
            }
        };
    });

// Criipto signature
builder.Services.AddSingleton<CriiptoSignaturesClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
    {
        return new CriiptoSignaturesClient(Environment.GetEnvironmentVariable("CRIIPTO_CLIENT_ID")!,
            Environment.GetEnvironmentVariable("CRIIPTO_CLIENT_SECRET")!);
    }
    else
    {
        string clientId = builder.Configuration["Criipto:ClientId"]!;
        string clientSecret = builder.Configuration["Criipto:ClientSecret"]!;
        return new CriiptoSignaturesClient(clientId, clientSecret);
    }
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISignatureService, SignatureService>();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });
    app.UseHttpsRedirection();
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});
app.UseHttpsRedirection();

app.UseCors("MyCorsPolicy");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.MapControllers();

app.Run();

async Task RedeemAuthorizationCodeAsync(AuthorizationCodeReceivedContext context)
{
    try
    {
        var configuration = await context.Options.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
        var tokenEndpoint = configuration.TokenEndpoint;

        // Log the TokenEndpoint to ensure it is correct
        Console.WriteLine($"TokenEndpoint: {tokenEndpoint}");

        // Ensure the TokenEndpoint is an absolute URI
        if (!Uri.IsWellFormedUriString(tokenEndpoint, UriKind.Absolute))
        {
            throw new InvalidOperationException("The TokenEndpoint is not a valid absolute URI.");
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        var authInfo = $"{context.TokenEndpointRequest.ClientId}:{context.TokenEndpointRequest.ClientSecret}";
        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
        var tokenEndpointRequest = context.TokenEndpointRequest.Clone();
        tokenEndpointRequest.ClientSecret = null;
        requestMessage.Content = new FormUrlEncodedContent(tokenEndpointRequest.Parameters);

        var responseMessage = await context.Backchannel.SendAsync(requestMessage);
        if (!responseMessage.IsSuccessStatusCode)
        {
            Console.WriteLine(await responseMessage.Content.ReadAsStringAsync());
            return;
        }

        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var message = new OpenIdConnectMessage(responseContent);
        context.HandleCodeRedemption(message);
    }
    catch (Exception exc)
    {
        Console.WriteLine($"An error occurred: {exc.Message}");
        throw; // Re-throw the exception to ensure proper error handling
    }
}

public partial class Program { }