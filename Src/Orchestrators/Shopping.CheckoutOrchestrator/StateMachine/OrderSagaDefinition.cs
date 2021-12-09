using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Definition;

namespace Shopping.OrderSagaOrchestrator.StateMachine
{
    public class OrderSagaDefinition : SagaDefinition<OrderStateData>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderStateData> sagaConfigurator)
        {
            // If using Azure Service Bus we need to tell the queue to use session
            if(endpointConfigurator is IServiceBusReceiveEndpointConfigurator sb)
            {
                sb.RequiresSession = true; // this is not available for Azure Basic tier
            }
        }
    }
}
