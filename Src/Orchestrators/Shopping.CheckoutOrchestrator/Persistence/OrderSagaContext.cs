using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Shopping.OrderSagaOrchestrator.Models;
using System.Collections.Generic;

namespace Shopping.OrderSagaOrchestrator.Persistence
{
    public class OrderSagaContext : SagaDbContext
    {

        public OrderSagaContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderStateDataMap(); }
        }
    }
}
