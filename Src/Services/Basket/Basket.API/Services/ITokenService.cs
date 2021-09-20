using System.Threading.Tasks;

namespace Basket.API.Services
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenForDownstreamServices();
    }
}