using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopping.OrderSagaOrchestrator.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Persistence
{
    // The SagaClassMap has a default mapping for the CorrelationId as the primary key.
    public class OrderStateDataMap : SagaClassMap<OrderStateData>
    {
        protected override void Configure(EntityTypeBuilder<OrderStateData> entity, ModelBuilder model)
        {
            entity.Property(order => order.OrderTotalPrice)
                .HasColumnType("decimal(18, 4)");

            entity.Property(x => x.CurrentState)
                .HasMaxLength(64);

            // If using Optimistic concurrency, otherwise remove this property
            //entity.Property(x => x.RowVersion).IsRowVersion();
        }
    }
}
