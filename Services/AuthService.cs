using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

using ztlme.Data;
using ztlme.Models;

namespace ztlme.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataContext _dataContext;
    
    public AuthService(IConfiguration configuration,
        IHttpContextAccessor contextAccessor, DataContext dataContext)
    {
        _httpContextAccessor = contextAccessor;
        _dataContext = dataContext;
    }

    public ServiceResponse<bool> CheckAuth()
    {
        var response = new ServiceResponse<bool>();
        response.Success = false;
        response.Data = false;
        
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return response;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return response;
        }

        response.Success = true;
        response.Data = true;
        return response;
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

            response.Success = true;
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