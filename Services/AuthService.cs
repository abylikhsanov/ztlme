using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

    public async Task<ServiceResponse<PersonSignUpResponse>> CheckIfCanBeSignedUp(PersonSignUp person)
    {
        var response = new ServiceResponse<PersonSignUpResponse>();
        var personSignUp = new PersonSignUpResponse();
        response.Success = true;
        response.Data = personSignUp;
        
        // Before checking credit score (it costs money, check if we have the person in the database
        var userDb = await _dataContext.Users.FirstOrDefaultAsync(user => user.UserName == person.PersonalNumber);
        if (userDb != null)
        {
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
            UserName = person.PersonalNumber
        };
        
        // For testing purposes, suppose only this credit check is okay
        if (person.PersonalNumber == "30070721151")
        {
            newUser.CreditScoreOk = true;
            response.Data.CanBeSignedUp = true;
        }

        _dataContext.Users.Add(newUser);
        await _dataContext.SaveChangesAsync();
        
        return response;
    }
    
}