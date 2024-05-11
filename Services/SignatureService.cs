using ztlme.Data;
using ztlme.Models;
using ztlme.Methods;
using Criipto.Signatures;
using Criipto.Signatures.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace ztlme.Services;

internal class SignatureService : ISignatureService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataContext _dataContext;
    private readonly CriiptoSignaturesClient _criiptoSignaturesClient;
    private readonly ILogger<SignatureService> _logger;
    private readonly IConfiguration _configuration;

    private List<DocumentInput> CreateDocument(byte[] file, string userId)
    {
        return
        [
            new DocumentInput
            {
                pdf = new PadesDocumentInput
                {
                    title = userId,
                    blob = file,
                    storageMode = DocumentStorageMode.Temporary
                }
            }
        ];
    }

    private List<EvidenceProviderInput> CreateEvidence()
    {
        return
        [
            new EvidenceProviderInput
            {
                criiptoVerify = new CriiptoVerifyProviderInput()
                {
                    acrValues = new List<string>()
                    {
                        "urn:grn:authn:no:bankid", "urn:grn:authn:no:bankid:substantial",
                        "urn:grn:authn:no:vipps"
                    },
                    alwaysRedirect = true
                }
            }
        ];
    }

    private async Task<string> GetSignLink(List<DocumentInput> documents, List<EvidenceProviderInput> evidenceProviderInputs)
    {
        var absoluteUri = _configuration["BackendURI"] + "/api/signature/success";
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            absoluteUri = Environment.GetEnvironmentVariable("BACKEND_URL") + "/api/signature/success";
        }

        var createSignatureOrderInput = new CreateSignatureOrderInput
        {
            title = "ztl.me fullmakt",
            documents = documents,
            evidenceProviders = evidenceProviderInputs,
            webhook = new CreateSignatureOrderWebhookInput()
            {
                url = absoluteUri,
                validateConnectivity = true
            }
        };
        _logger.LogInformation($"The webhook URI is: {absoluteUri}");
        var signatureOrder = await _criiptoSignaturesClient.CreateSignatureOrder(createSignatureOrderInput);
        var signatory = await _criiptoSignaturesClient.AddSignatory(signatureOrder.id);
        return signatory.href;
    }

    public SignatureService(IHttpContextAccessor httpContextAccessor, DataContext dataContext,
        CriiptoSignaturesClient criiptoSignaturesClient, ILogger<SignatureService> logger,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _dataContext = dataContext;
        _criiptoSignaturesClient = criiptoSignaturesClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ServiceResponse<string>> SignDocument()
    {
        var response = new ServiceResponse<string>();
        response.Success = false;

        try
        {
            var s3Service = new S3Service();
            byte[] fileData = await s3Service.DownloadFileAsync("ztl.me", "Document.pdf");

            // Checking the file content independently from the controller
            if (fileData.Length == 0)
            {
                response.Message = "File is either null or empty";
                return response;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst("socialno")?.Value;
            if (userId.IsNullOrEmpty())
            {
                _logger.LogError("userId (personal number) is null or empty");
                response.Message = "userId (personal number) is null or empty";
                return response;
            }

            _logger.LogInformation($"userId is: {userId}");

            var documents = CreateDocument(fileData, userId!);
            var evidenceProviders = CreateEvidence();
            var link = await GetSignLink(documents, evidenceProviders);
            _logger.LogInformation($"Have succesfully obtained with data: {link}");
            response.Data = link;
            response.Message = "Successfully obtained signature link";
            response.Success = true;
            return response;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ServiceResponse<string>> SignatureSuccess(string eventToken, string signatureOrderId)
    {
        var response = new ServiceResponse<string>();
        _logger.LogInformation($"Event: {eventToken}");
        if (eventToken != "SIGNATORY_SIGNED")
        {
            return response;
        }

        response.Success = false;

        // Close signature and store in the DB
        try
        {
            // Close the signature
            if (signatureOrderId.IsNullOrEmpty())
            {
                _logger.LogError($"During SIGNATORY_SIGNED, received null or empty signatoryId");
                return response;
            }

            var closedSignatureOrder = await _criiptoSignaturesClient.CloseSignatureOrder(signatureOrderId!);
            if (closedSignatureOrder.documents.Count == 0)
            {
                _logger.LogError($"Closed signature returned a document array of size 0");
                return response;
            }

            var document = closedSignatureOrder.documents[0];

            // Save blob to the user in DB
            var user = _httpContextAccessor.HttpContext?.User;
            var username = document.title;


            _logger.LogInformation($"Personal number: {username}");

            if (username.IsNullOrEmpty())
            {
                _logger.LogError($"No user found with personal number: {username}");
                return response;
            }

            var userDb = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (userDb is null)
            {
                _logger.LogError($"No user found in db with personal number: {username}");
                return response;
            }

            // Convert to base64
            userDb.SignedBlob = Convert.ToBase64String(document.blob);

            _logger.LogInformation($"Updating or adding a user with username: {username}");
            await _dataContext.SaveChangesAsync();
            response.Success = true;
        }
        catch (Exception e)
        {
            _logger.LogError("Exceptions: " + e.Message);
            response.Success = false;
            return response;
        }

        return response;
    }

    public async Task<ServiceResponse<string>> UnAuthSignDocument()
    {
        var response = new ServiceResponse<string>();
        response.Success = false;

        try
        {
            var s3Service = new S3Service();
            byte[] fileData = await s3Service.DownloadFileAsync("ztl.me", "Document.pdf");

            // Checking the file content independently from the controller
            if (fileData.Length == 0)
            {
                response.Message = "File is either null or empty";
                return response;
            }

            var userId = "";
            _logger.LogInformation($"userId is: {userId}");

            var documents = CreateDocument(fileData, userId!);
            var evidenceProviders = CreateEvidence();
            var link = await GetSignLink(documents, evidenceProviders);
            _logger.LogInformation($"Have succesfully obtained with data: {link}");
            response.Data = link;
            response.Message = "Successfully obtained signature link";
            response.Success = true;
            return response;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ServiceResponse<bool>> UnAuthSignatureSuccess(string eventToken, string signatureOrderId)
    {
        var response = new ServiceResponse<bool>();
        _logger.LogInformation($"Event: {eventToken}");
        if (eventToken != "SIGNATORY_SIGNED")
        {
            return response;
        }

        response.Success = false;

        // Close signature and store in the DB
        try
        {
            // Close the signature
            if (signatureOrderId.IsNullOrEmpty())
            {
                _logger.LogError($"During SIGNATORY_SIGNED, received null or empty signatoryId");
                return response;
            }

            var closedSignatureOrder = await _criiptoSignaturesClient.CloseSignatureOrder(signatureOrderId!);
            if (closedSignatureOrder.documents.Count == 0)
            {
                _logger.LogError($"Closed signature returned a document array of size 0");
                return response;
            }

            // Extract username from the jwks
            var document = closedSignatureOrder.documents[0];
            var handler = new JwtSecurityTokenHandler();
            string? username = null;
            string? firstName = null;
            string? lastName = null;
           
            // document.blob is a byte array containing the signed/closed PDF
            foreach (var signature in document.signatures)
            {
                if (signature is JWTSignature jwtSignature)
                {
                    try
                    {
                        JsonWebKeySet jsonWebKeySet = new JsonWebKeySet(jwtSignature.jwks);
                        var principal = handler.ValidateToken(jwtSignature.jwt, new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKeys = jsonWebKeySet.Keys,
                                ValidateIssuer = false,
                                ValidateAudience = false
                            },
                            out var validatedToken);
                        var jwtTokenValidated = validatedToken as JwtSecurityToken;

                        if (jwtTokenValidated != null)
                        {
                            username = jwtTokenValidated.Claims.FirstOrDefault(claim => claim.Type == "socialno")
                                ?.Value;
                            firstName = jwtTokenValidated.Claims.FirstOrDefault(claim => claim.Type == "givenname")
                                ?.Value;
                            lastName = jwtTokenValidated.Claims.FirstOrDefault(claim => claim.Type == "surname")
                                ?.Value;
                        }
                    }
                    catch (SecurityTokenException ste)
                    {
                        Console.WriteLine("Token validation failed: " + ste.Message);
                    }
                    
                }
            }

            // Save blob to the user in DB
            if (username.IsNullOrEmpty())
            {
                _logger.LogError("Username is null or empty after closing the document after signature");
                return response;
            }

            _logger.LogInformation($"Personal number: {username}");

            var userDb = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (userDb is null)
            {
                _logger.LogError($"No user found in db with personal number: {username}");
                return response;
            }

            if (!userDb.CreditScoreOk)
            {
                _logger.LogError($"User with personam number: {username} have a low score, cannot sign the document");
                return response;
            }

            // Convert to base64 and update user
            userDb.SignedBlob = Convert.ToBase64String(document.blob);
            userDb.FirstName = firstName;
            userDb.LastName = lastName;

            _logger.LogInformation($"Updating or adding a user with username: {username}");
            await _dataContext.SaveChangesAsync();
            response.Success = true;
        }
        catch (Exception e)
        {
            _logger.LogError("Exceptions: " + e.Message);
            response.Success = false;
            return response;
        }

        return response;
    }
}