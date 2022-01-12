using Automatonymous;
using EventBus.Messages.Events.Order;
using MassTransit;

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
        public Event<DispatchOrder> DispatchOrderEvent { get; private set; }
        public Event<OrderDispatched> OrderDispatchedEvent { get; private set; }
        public Event<OrderDelivered> OrderDeliveredEvent { get; private set; }
        public Event<OrderNotDelivered> OrderNotDeliveredEvent { get; private set; }
        public Event<OrderCanceled> OrderCanceledEvent { get; private set; }
        public Event<Fault<BillOrder>> BillOrderFaultEvent { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            // An event is something that happened which may result in a state change. 
            // An event can add or update instance data, as well as changing an instance's current state. 
            // The Event<T> is generic, where T must be a valid message type.
            Event(() => OrderPlacedEvent);
            Event(() => BillOrderEvent);
            Event(() => OrderBilledEvent);
            Event(() => DispatchOrderEvent);
            Event(() => OrderDispatchedEvent);
            Event(() => OrderDeliveredEvent);
            Event(() => OrderNotDeliveredEvent);
            Event(() => OrderCanceledEvent);
            Event(() => BillOrderFaultEvent, x => x
                .CorrelateById(m => m.Message.Message.CorrelationId)); // Fault<T> includes the original message. We must correlate the fault event with the key from state object

            // Behavior is what happens when an event occurs during a state.
            // The Initially block is used to define the behavior of the OrderPlacedEvent during the Initial state.
            // When a SubmitOrder message is consumed and an instance with a CorrelationId matching the OrderId is not found, a new instance will be created in the Initial state.
            // The TransitionTo activity transitions the instance to the Submitted state, after which the instance is persisted using the saga repository.
            Initially(
                When(OrderPlacedEvent)
                    .Then(context =>
                    {
                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.OrderCreationDate = context.Data.OrderCreationDate;
                        context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;
                        context.Instance.OrderTotalPrice = context.Data.OrderTotalPrice;
                        context.Instance.CustomerUsername = context.Data.CustomerUsername;
                    })
                    .TransitionTo(OrderPlaced)
            );

            // Order Sent for billing
            During(OrderPlaced,
                Ignore(OrderBilledEvent),
                When(BillOrderEvent)
                    .TransitionTo(OrderSentForBilling),
                When(OrderCanceledEvent)
                    .Then(context => context.Instance.OrderCancellationDate = context.Data.OrderCancellationDate)
                        .TransitionTo(OrderCanceled));

            // Billed order
            During(OrderSentForBilling,
                Ignore(OrderDispatchedEvent),
                When(OrderCanceledEvent)
                    .Then(context => context.Instance.OrderCancellationDate = context.Data.OrderCancellationDate)
                        .TransitionTo(OrderCanceled),
                When(OrderBilledEvent)
                    .TransitionTo(OrderBilled),
                When(BillOrderFaultEvent) // rollback will be handled inside BillOrderFaultConsumer
                    .TransitionTo(OrderFailedToBeBilled));

            // Waiting to be dispatched
            During(OrderBilled,
                Ignore(OrderDispatchedEvent),
                When(DispatchOrderEvent)
                    .TransitionTo(OrderWaitingToBeDispatched));

            // Order dispatched
            During(OrderWaitingToBeDispatched,
                Ignore(OrderBilledEvent),
                When(OrderDispatchedEvent)
                    .TransitionTo(OrderDispatched));

            // Order (not)delivered
            During(OrderDispatched,
                When(OrderNotDeliveredEvent)
                    .TransitionTo(OrderNotDelivered)
                    .Finalize(),
                When(OrderDeliveredEvent)
                    .TransitionTo(OrderDelivered)
                    .Finalize());

            SetCompletedWhenFinalized();
        }
    }
}
