namespace EventBus.Messages.Common
{
    public static class EventBusConstants
    {
        public const string BasketCheckoutQueue = "basketcheckout-queue";
        public const string OrderSagaQueue = "ordersaga-queue";
        public const string OrderBillingQueue = "orderbilling-queue";
        public const string OrderBillingRollbackQueue = "orderbillingrollback-queue";
        public const string RollbackOrderQueue = "rollbackorder-queue";
        public const string OrderStatusNotifierQueue = "orderstatusnotifier-queue";
        public const string OrderDeliveryQueue = "orderdelivery-queue";
    }
}
