using System.ComponentModel;

namespace Ordering.Domain.Common
{
    public enum OrderStatus
    {
        [Description("Pending")]
        PENDING,

        [Description("Order Paid")]
        ORDER_PAID,

        [Description("Order Dispatched")]
        ORDER_DISPATCHED,

        [Description("Order Delivered")]
        ORDER_DELIVERED,

        [Description("Order Not Delivered")]
        ORDER_NOT_DELIVERED,

        [Description("Order Canceled")]
        ORDER_CANCELED,

        [Description("Order Failed To Be Billed")]
        ORDER_FAILED_TO_BE_BILLED
    }
}
