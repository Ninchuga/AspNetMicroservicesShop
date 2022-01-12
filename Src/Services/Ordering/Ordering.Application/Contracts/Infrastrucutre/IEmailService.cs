using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Ordering.Application.Contracts.Infrastrucutre
{
    public interface IEmailService
    {
        Task SendMailFor(string email, string userName, Guid orderId);
        Task<bool> SendEmail(Email email);
    }
}
