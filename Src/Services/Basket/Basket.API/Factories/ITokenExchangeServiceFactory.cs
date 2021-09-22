using Basket.API.Services.Tokens;

namespace Basket.API.Factories
{
    public interface ITokenExchangeServiceFactory
    {
        ITokenExchangeService GetTokenExchangeServiceInstance(string serviceName);
    }
}