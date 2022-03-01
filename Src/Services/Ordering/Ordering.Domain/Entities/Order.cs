using Ordering.Domain.Common;
using Ordering.Domain.Exceptions;
using Ordering.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ordering.Domain.Entities
{
    public class Order : BaseEntity<Guid>, IAggregateRoot
    {
        public Guid UserId { get; }
        public string UserName { get; }
        public decimal TotalPrice { get; }
        public OrderStatus OrderStatus { get; private set; }
        public DateTime OrderDate { get; }
        public DateTime? OrderCancellationDate { get; private set; }

        // BillingAddress
        public Address Address { get; private set; }
        public PaymentData PaymentData { get; private set; }

        private List<OrderItem> _orderItems;
        public IReadOnlyList<OrderItem> OrderItems => _orderItems;

        public Order(Guid orderId, Guid userId, string userName, decimal totalPrice, OrderStatus orderStatus, DateTime orderDate, Address address, PaymentData paymentData)
            : this(orderId, userId, userName, totalPrice, orderStatus, orderDate)
        {
            Address = address;
            PaymentData = paymentData;
        }

        /// <summary>
        /// EF constructor
        /// </summary>
        private Order(Guid id, Guid userId, string userName, decimal totalPrice, OrderStatus orderStatus, DateTime orderDate)
            : base(id)
        {
            UserId = userId;
            UserName = userName;
            TotalPrice = totalPrice;
            OrderStatus = orderStatus;
            OrderDate = orderDate;
            _orderItems = new List<OrderItem>();
        }

        public void AddOrderItem(string productId, string productName, decimal itemPrice, decimal discount, int quantity = 1)
        {
            var existingOrderForProduct = _orderItems.Where(o => o.ProductId == productId)
                .SingleOrDefault();

            if (existingOrderForProduct != null)
            {
                //if previous line exist modify it with higher discount  and quantity..

                //if (discount > existingOrderForProduct.GetCurrentDiscount())
                //{
                //    existingOrderForProduct.SetNewDiscount(discount);
                //}

                existingOrderForProduct.UpdateQuantity(quantity);
            }
            else
            {
                var orderItem = new OrderItem(productId, quantity, itemPrice, productName, discount);
                _orderItems.Add(orderItem);
            }
        }

        public void SetOrderToPaid()
        {
            OrderStatus = OrderStatus.ORDER_PAID;
            PaymentData = PaymentData.PaidOrder();
        }

        public void SetOrderStatusToDelivered() => OrderStatus = OrderStatus.ORDER_DELIVERED;

        public void SetOrderStatusToDispatched() => OrderStatus = OrderStatus.ORDER_DISPATCHED;

        public void SetOrderStatusToPending() => OrderStatus = OrderStatus.PENDING;

        public void SetCanceledOrderStatusAndTime()
        {
            if (OrderStatus == OrderStatus.ORDER_CANCELED)
                throw new OrderCancelationException("Order is already canceled.");

            bool isValidToCancel = OrderDate.AddHours(24) > DateTime.UtcNow;
            if (!isValidToCancel && (OrderStatus == OrderStatus.PENDING || OrderStatus == OrderStatus.ORDER_FAILED_TO_BE_BILLED))
                throw new OrderCancelationException("You can only cancel your order in the first 24h when the order was placed.");

            if (OrderStatus != OrderStatus.PENDING && OrderStatus != OrderStatus.ORDER_FAILED_TO_BE_BILLED)
                throw new OrderCancelationException("Order is already billed and can't be canceled.");

            OrderStatus = OrderStatus.ORDER_CANCELED;
            OrderCancellationDate = DateTime.UtcNow;
        }
    }
}
