using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Services
{
    public interface ITokenExchangeService
    {
        Task<string> ExchangeAccessToken(string tokenExchangeCacheKey, string serviceScopes, string subjectAccessToken);
    }
}