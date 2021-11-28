using System;
using System.Threading.Tasks;

namespace Ordering.Application.Services
{
    public interface ITokenValidationService
    {
        Task<bool> ValidateTokenAsync(string tokenToValidate, DateTime messageSentTime);
    }
}