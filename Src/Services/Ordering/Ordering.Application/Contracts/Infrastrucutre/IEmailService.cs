using Ordering.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.Contracts.Infrastrucutre
{
    public interface IEmailService
    {
        Task<bool> SendEmail(Email email);
    }
}
