using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System.Threading.Tasks;

namespace Ordering.Application.Contracts.Infrastrucutre
{
    public interface IEmailService
    {
        Task SendMailFor(Order order);
        Task<bool> SendEmail(Email email);
    }
}
