using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ztlme.Data;
using ztlme.Dtos;
using ztlme.Models;

namespace ztlme.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataContext _dataContext;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(IConfiguration configuration,
        IHttpContextAccessor contextAccessor, DataContext dataContext,
        ILogger<AuthService> logger)
    {
        _httpContextAccessor = contextAccessor;
        _dataContext = dataContext;
        _logger = logger;
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
    
    public async Task<ServiceResponse<string>> RegisterCriiptoUserSuccess()
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

    public async Task<ServiceResponse<AddUserLandingResDto>> CheckIfCanBeSignedUp(string personalNumber)
    {
        var response = new ServiceResponse<AddUserLandingResDto>();
        response.Success = true;
        
        // Before checking credit score (it costs money, check if we have the person in the database
        var userDb = await _dataContext.Users.FirstOrDefaultAsync(user => user.UserName == personalNumber);
        if (userDb != null)
        {
            response.Data = userDb.ToAddUserLandingResDto();
            response.Data.CreditScoreApiCalled = false;
            if (!userDb.CreditScoreOk)
            {
                return response;
            }

            if (!userDb.SignedBlob.IsNullOrEmpty())
            {
                Console.WriteLine("Ok");
                response.Data.CanBeSignedUp = true;
                response.Data.DocumentSigned = true;
                return response;
            }

            response.Data.CanBeSignedUp = true;
            return response;
        }
        
        // Perform credit score, new user.
        var newUser = new User
        {
            UserName = personalNumber,
            CreditScoreOk = false
        };
        response.Data = newUser.ToAddUserLandingResDto();
        
        // For testing purposes, suppose only this credit check is okay
        if (personalNumber == "30070721151")
        {
            newUser.CreditScoreOk = true;
            response.Data.CanBeSignedUp = true;
        }

        _dataContext.Users.Add(newUser);
        await _dataContext.SaveChangesAsync();
        
        return response;
    }

    public async Task<ServiceResponse<AddUserLandingNoCheckResDto>> AddUserLandingPage(AddUserLandingNoCheckDto landingUser)
    {
        var response = new ServiceResponse<AddUserLandingNoCheckResDto>();
        response.Success = true;

        var user = landingUser.FromAddUserLandingNoCheck();
        _dataContext.Users.Add(user);
        await _dataContext.SaveChangesAsync();

        return response;
    }

    public async Task<RegisterUserRes> RegisterSignicatUser()
    {
        var response = new RegisterUserRes();


        return response;
    }
    
}