using System.Threading.Tasks;

namespace Basket.API.Services.Tokens
{
    public interface ITokenExchangeService
    {
        Task<string> GetAccessTokenForDownstreamService(string downstreamApiAccessTokenCacheKey, string downstreamApiScopes);
    }
}