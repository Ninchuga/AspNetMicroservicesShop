using System.Threading.Tasks;

namespace Basket.API.Services.Tokens
{
    public class NullTokenExchangeService : ITokenExchangeService
    {
        public async Task<string> GetAccessTokenForDownstreamService()
        {
            return await Task.FromResult(string.Empty);
        }
    }
}
