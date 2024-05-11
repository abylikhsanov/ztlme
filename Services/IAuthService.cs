using ztlme.Dtos;
using ztlme.Models;

namespace ztlme.Services;

public interface IAuthService
{
    ServiceResponse<bool> CheckAuth();
    Task<ServiceResponse<string>> AuthSuccessBankId();
    Task<ServiceResponse<SignupUserResDto>> CheckIfCanBeSignedUp(string personalNumber);
}