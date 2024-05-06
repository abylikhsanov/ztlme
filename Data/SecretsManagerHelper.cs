using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace ztlme.Data;

public class SecretsManagerHelper
{
    private readonly IAmazonSecretsManager _secretsManager;

    public SecretsManagerHelper(IAmazonSecretsManager secretsManager)
    {
        _secretsManager = secretsManager;
    }

    public async Task<string> GetSecretAsync(string secretId)
    {
        var response = await _secretsManager.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretId
        });

        return response.SecretString;
    }
}