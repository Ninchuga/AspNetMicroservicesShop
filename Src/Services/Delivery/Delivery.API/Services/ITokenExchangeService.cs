using System.Threading.Tasks;

namespace Delivery.API.Services
{
    public interface ITokenExchangeService
    {
        Task<string> ExchangeAccessToken(string tokenExchangeCacheKey, string serviceScopes, string subjectAccessToken);
    }
}