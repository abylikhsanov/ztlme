using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

using ztlme.Data;
using ztlme.Models;

namespace ztlme.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataContext _dataContext;
    
    private int? GetUserId()
    {
        try
        {
            var userIdValue = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdValue, out var userId))
            {
                return userId;
            }
            return null;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Exception during GetUserId: {exception.Message}");
            return null;
        }
    }
    
    public AuthService(IConfiguration configuration,
        IHttpContextAccessor contextAccessor, DataContext dataContext)
    {
        _configuration = configuration;
        _httpContextAccessor = contextAccessor;
        _dataContext = dataContext;
    }

    
    public async Task<ServiceResponse<string>> AuthSuccessBankId()
    {
        var response = new ServiceResponse<string>();

        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var username = user?.FindFirst("socialno")?.Value;
            var firstname = user?.FindFirst("givenname")?.Value;
            var lastname = user?.FindFirst("surname")?.Value;
            
            Console.WriteLine($"Username: {username}");

            if (username is null || firstname is null || lastname is null)
            {
                response.Success = false;
                return response;
            }

            var userDb = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (userDb is null)
            {
                _dataContext.Users.Add(new User
                {
                    FirstName = user!.FindFirst("givenname")!.Value,
                    LastName = user!.FindFirst("surname")!.Value,
                    UserName = username
                });
                await _dataContext.SaveChangesAsync();
            }

            response.Data = "Logged in";

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            response.Success = false;
            return response;
        }
        return response;
    }
    
}