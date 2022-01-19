using Automatonymous;
using System;

namespace Shopping.OrderSagaOrchestrator.StateMachine
{
    /// <summary>
    /// This is Saga State Machine instance.
    /// An instance contains the data for a state machine instance.
    /// A new instance is created for every consumed initial event where an existing instance with the same CorrelationId was not found.
    /// A saga repository is used to persist instances.
    /// Instances are classes, and must implement the SagaStateMachineInstance interface.
    /// </summary>
    public class OrderStateData : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime? OrderCreationDate { get; set; }
        public DateTime? OrderCancellationDate { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentCardNumber { get; set; }
        public decimal OrderTotalPrice { get; set; }
        public string CustomerUsername { get; set; }

        // If using Optimistic concurrency, this property is required
        public byte[] RowVersion { get; set; }
    }
}
