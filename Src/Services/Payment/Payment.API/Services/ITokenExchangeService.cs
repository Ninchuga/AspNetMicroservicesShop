using System.Threading.Tasks;

namespace Payment.API.Services
{
    public interface ITokenExchangeService
    {
        Task<string> ExchangeAccessToken(string tokenExchangeCacheKey, string serviceScopes, string subjectAccessToken);
    }
}