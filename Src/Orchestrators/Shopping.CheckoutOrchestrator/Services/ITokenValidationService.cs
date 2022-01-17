using System;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Services
{
    public interface ITokenValidationService
    {
        Task<bool> ValidateTokenAsync(string tokenToValidate, DateTime messageSentTime);
    }
}