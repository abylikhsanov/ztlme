using ztlme.Models;

namespace ztlme.Services;

public interface IAuthService
{
    Task<ServiceResponse<string>> AuthSuccessBankId();
}