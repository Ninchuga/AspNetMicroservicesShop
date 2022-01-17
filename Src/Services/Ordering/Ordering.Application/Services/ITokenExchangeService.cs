using System.Threading.Tasks;

namespace Ordering.Application.Services
{
    public interface ITokenExchangeService
    {
        Task<string> ExchangeAccessToken(string tokenExchangeCacheKey, string serviceScopes);
    }
}