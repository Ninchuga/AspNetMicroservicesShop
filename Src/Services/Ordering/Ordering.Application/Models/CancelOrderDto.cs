using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Application.Models
{
    public class CancelOrderDto
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
    }
}
