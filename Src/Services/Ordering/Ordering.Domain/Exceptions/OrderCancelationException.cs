using System;

namespace Ordering.Domain.Exceptions
{
    public class OrderCancelationException : Exception
    {
        public OrderCancelationException()
        { }

        public OrderCancelationException(string message)
            : base(message)
        { }
    }
}
