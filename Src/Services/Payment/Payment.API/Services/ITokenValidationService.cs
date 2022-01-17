using System;
using System.Threading.Tasks;

namespace Payment.API.Services
{
    public interface ITokenValidationService
    {
        Task<bool> ValidateTokenAsync(string tokenToValidate, DateTime messageSentTime);
    }
}