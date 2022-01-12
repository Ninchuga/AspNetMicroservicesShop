namespace Ordering.Application.Models.Responses
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
