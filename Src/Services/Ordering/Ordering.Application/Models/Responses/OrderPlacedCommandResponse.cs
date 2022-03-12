using System;

namespace Ordering.Application.Models.Responses
{
    public class OrderPlacedCommandResponse
    {
        public OrderPlacedCommandResponse(bool success, string errorMessage, Guid orderId = default)
        {
            Success = success;
            ErrorMessage = errorMessage;
            OrderId = orderId;
        }

        public Guid OrderId { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }
    }
}
