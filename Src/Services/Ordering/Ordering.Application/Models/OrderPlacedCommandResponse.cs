using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Application.Models
{
    public class OrderPlacedCommandResponse
    {
        public OrderPlacedCommandResponse(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; }
        public string ErrorMessage { get; }
    }
}
