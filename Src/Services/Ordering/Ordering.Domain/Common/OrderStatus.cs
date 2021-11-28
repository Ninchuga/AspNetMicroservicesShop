namespace Ordering.Domain.Common
{
    public enum OrderStatus
    {
        PENDING,
        ORDER_BILLED,
        ORDER_DISPATCHED,
        ORDER_DELIVERED,
        ORDER_NOT_DELIVERED,
        ORDER_CANCELED,
        ORDER_FAILED_TO_BE_BILLED
    }
}
