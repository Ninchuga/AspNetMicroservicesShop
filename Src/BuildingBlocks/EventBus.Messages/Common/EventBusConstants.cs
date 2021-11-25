using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Common
{
    public static class EventBusConstants
    {
        public const string BasketCheckoutQueue = "basketcheckout-queue";
        public const string ApplyBasketItemsDiscountQueue = "basketitemsdiscount-queue";
        public const string BasketItemsDiscountAppliedQueue = "basketitemsdiscountapplied-queue";
        public const string CreateOrderQueue = "createorder-queue";
        public const string OrderSagaRepliesQueue = "ordersagareplies-queue";


    }
}
