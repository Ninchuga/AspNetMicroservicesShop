using System.Threading.Tasks;

namespace EmailFunction.Services
{
    public interface IKeyVaultManager
    {
        Task<string> GetSecret(string secretName);
    }
}
