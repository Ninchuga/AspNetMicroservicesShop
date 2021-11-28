using Automatonymous;
using EventBus.Messages.Events.Order;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.StateMachine
{
    /// <summary>
    /// A state machine defines the states, events, and behavior of a finite state machine.
    /// Implemented as a class, which is derived from MassTransitStateMachine<T>, a state machine is created once, 
    /// and then used to apply event triggered behavior to state machine instances.
    /// </summary>
    public class OrderStateMachine : MassTransitStateMachine<OrderStateData>
    {
        public State Pending { get; private set; }
        public State OrderPlaced { get; private set; }
        public State OrderSentForBilling { get; private set; }
        public State OrderBilled { get; private set; }
        public State OrderFailedToBeBilled { get; private set; }
        public State OrderWaitingToBeDispatched { get; private set; }
        public State OrderDispatched { get; private set; }
        public State OrderDelivered { get; private set; }
        public State OrderNotDelivered { get; private set; }
        public State OrderCanceled { get; private set; }

        public Event<OrderPlaced> OrderPlacedEvent { get; private set; }
        public Event<BillOrder> BillOrderEvent { get; private set; }
        public Event<OrderBilled> OrderBilledEvent { get; private set; }
        public Event<OrderFailedToBeBilled> OrderFailedToBeBilledEvent { get; private set; }
        public Event<DispatchOrder> DispatchOrderEvent { get; private set; }
        public Event<OrderDispatched> OrderDispatchedEvent { get; private set; }
        public Event<OrderDelivered> OrderDeliveredEvent { get; private set; }
        public Event<OrderNotDelivered> OrderNotDeliveredEvent { get; private set; }
        public Event<OrderCanceled> OrderCanceledEvent { get; private set; }
        public Event<Fault<OrderBilled>> OrderBillingFaultedEvent { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            // An event is something that happened which may result in a state change. 
            // An event can add or update instance data, as well as changing an instance's current state. 
            // The Event<T> is generic, where T must be a valid message type.
            Event(() => OrderPlacedEvent);
            Event(() => BillOrderEvent);
            Event(() => OrderBilledEvent);
            Event(() => OrderFailedToBeBilledEvent);
            Event(() => DispatchOrderEvent);
            Event(() => OrderDispatchedEvent);
            Event(() => OrderDeliveredEvent);
            Event(() => OrderNotDeliveredEvent);
            Event(() => OrderCanceledEvent);
            Event(() => OrderBillingFaultedEvent, x => x.CorrelateById(m => m.CorrelationId.Value));

            // Behavior is what happens when an event occurs during a state.
            // The Initially block is used to define the behavior of the OrderPlacedEvent during the Initial state.
            // When a SubmitOrder message is consumed and an instance with a CorrelationId matching the OrderId is not found, a new instance will be created in the Initial state.
            // The TransitionTo activity transitions the instance to the Submitted state, after which the instance is persisted using the saga repository.
            Initially(
            When(OrderPlacedEvent)
                .Then(context =>
                {
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.OrderCreationDateTime = context.Data.OrderCreationDateTime;
                    context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;
                    context.Instance.OrderTotalPrice = context.Data.OrderTotalPrice;
                    context.Instance.CustomerUsername = context.Data.CustomerUsername;
                })
                .TransitionTo(OrderPlaced)
                .Publish(context =>
                    {
                        return new BillOrder
                        {
                            OrderId = context.Instance.OrderId,
                            CorrelationId = context.Instance.CorrelationId,
                            OrderCreationDateTime = context.Instance.OrderCreationDateTime,
                            PaymentCardNumber = context.Instance.PaymentCardNumber,
                            OrderTotalPrice = context.Data.OrderTotalPrice,
                            CustomerUsername = context.Data.CustomerUsername
                    };
                }));

            // Order Sent for billing
            During(OrderPlaced,
                When(BillOrderEvent)
                    .TransitionTo(OrderSentForBilling),
                When(OrderCanceledEvent)
                    .Then(context => context.Instance.OrderCancelDateTime = DateTime.Now)
                        .TransitionTo(OrderCanceled)
                            .Publish(context =>
                            {
                                return new OrderStatusUpdated
                                {
                                    CorrelationId = context.Instance.CorrelationId,
                                    OrderId = context.Instance.OrderId,
                                    OrderStatus = OrderStatus.ORDER_CANCELED.ToString()
                                };
                            }));

            // Billed order
            During(OrderSentForBilling,
                When(OrderFailedToBeBilledEvent)
                    .Then(context => context.Instance.OrderCancelDateTime = DateTime.Now)
                        .TransitionTo(OrderFailedToBeBilled)
                            .Publish(context =>
                            {
                                return new OrderStatusUpdated
                                {
                                    CorrelationId = context.Instance.CorrelationId,
                                    OrderId = context.Instance.OrderId,
                                    OrderStatus = OrderStatus.ORDER_FAILED_TO_BE_BILLED.ToString()
                                };
                            }),
                When(OrderBilledEvent)
                    .TransitionTo(OrderBilled)
                    .Publish(context =>
                    {
                        return new OrderStatusUpdated
                        {
                            CorrelationId = context.Instance.CorrelationId,
                            OrderId = context.Instance.OrderId,
                            OrderStatus = OrderStatus.ORDER_BILLED.ToString()
                        };
                    })
                    .Publish(context =>
                    {
                        return new DispatchOrder
                        {
                            OrderId = context.Instance.OrderId,
                            CorrelationId = context.Instance.CorrelationId,
                            OrderCreationDateTime = context.Instance.OrderCreationDateTime,
                            PaymentCardNumber = context.Instance.PaymentCardNumber,
                            OrderTotalPrice = context.Data.OrderTotalPrice
                        };
                    }),
                When(OrderBillingFaultedEvent)
                    .Then(context => 
                    {
                        var exceptions = context.Data.Exceptions;
                        var message = context.Data.Message;
                    })
                    .TransitionTo(OrderFailedToBeBilled));

            // Waiting to be dispatched
            During(OrderBilled,
                When(DispatchOrderEvent)
                    .TransitionTo(OrderWaitingToBeDispatched));

            // Order (not)dispatched
            During(OrderWaitingToBeDispatched,
                When(OrderDispatchedEvent)
                    .TransitionTo(OrderDispatched)
                        .Publish(context =>
                        {
                            return new OrderStatusUpdated
                            {
                                CorrelationId = context.Instance.CorrelationId,
                                OrderId = context.Instance.OrderId,
                                OrderStatus = OrderStatus.ORDER_DISPATCHED.ToString()
                            };
                        }));

            // Order (not)delivered
            During(OrderDispatched,
                When(OrderNotDeliveredEvent)
                    .TransitionTo(OrderNotDelivered)
                    .Publish(context =>
                    {
                        return new OrderStatusUpdated
                        {
                            CorrelationId = context.Instance.CorrelationId,
                            OrderId = context.Instance.OrderId,
                            OrderStatus = OrderStatus.ORDER_NOT_DELIVERED.ToString()
                        };
                    })
                    .Finalize(),
                When(OrderDeliveredEvent)
                    .TransitionTo(OrderDelivered)
                    .Publish(context =>
                    {
                        return new OrderStatusUpdated
                        {
                            CorrelationId = context.Instance.CorrelationId,
                            OrderId = context.Instance.OrderId,
                            OrderStatus = OrderStatus.ORDER_DELIVERED.ToString()
                        };
                    })
                    .Finalize());
        }
    }
}
