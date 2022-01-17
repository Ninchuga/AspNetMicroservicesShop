using System;
using System.Threading.Tasks;

namespace Delivery.API.Services
{
    public interface ITokenValidationService
    {
        Task<bool> ValidateTokenAsync(string tokenToValidate, DateTime messageSentTime);
    }
}