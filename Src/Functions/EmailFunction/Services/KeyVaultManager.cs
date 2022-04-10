using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;

namespace EmailFunction.Services
{
    public class KeyVaultManager : IKeyVaultManager
    {
        private readonly SecretClient _secretClient;

        public KeyVaultManager(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        public async Task<string> GetSecret(string secretName)
        {
            var response = await _secretClient.GetSecretAsync(secretName);
            var secret = response.Value;
            return secret.Value;
        }
    }
}
