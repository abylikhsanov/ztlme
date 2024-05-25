using ztlme.Dtos;
using ztlme.Models;

namespace ztlme.Services;

public interface IAuthService
{
    ServiceResponse<bool> CheckAuth();
    Task<ServiceResponse<string>> RegisterCriiptoUserSuccess();
    Task<ServiceResponse<AddUserLandingResDto>> CheckIfCanBeSignedUp(string personalNumber);
    Task<ServiceResponse<AddUserLandingNoCheckResDto>> AddUserLandingPage(AddUserLandingNoCheckDto landingUser);
    Task<RegisterUserRes> RegisterSignicatUser();
}