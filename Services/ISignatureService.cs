using ztlme.Models;

namespace ztlme.Services;

public interface ISignatureService
{
    Task<ServiceResponse<string>> SignDocument();
    Task<ServiceResponse<string>> SignatureSuccess(string eventToken, string signatureOrderId);
}