using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Exceptions
{
    public class PaymentDataException : Exception
    {
        public PaymentDataException()
        { }

        public PaymentDataException(string message)
            : base(message)
        { }

        public PaymentDataException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
