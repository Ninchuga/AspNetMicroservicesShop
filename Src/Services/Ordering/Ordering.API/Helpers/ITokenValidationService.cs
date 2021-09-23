using System;
using System.Threading.Tasks;

namespace Ordering.API.Helpers
{
    public interface ITokenValidationService
    {
        Task<bool> ValidateTokenAsync(string tokenToValidate, DateTime messageSentTime);
    }
}