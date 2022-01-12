using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Infrastructure.EntityConfigurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired();

            builder
                .Property(o => o.UserId)
                .IsRequired();

            builder
                .Property(o => o.UserName)
                .IsRequired();

            builder
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18, 4)")
                .IsRequired();

            builder
                .Property(o => o.OrderStatus)
                .HasConversion<string>()
                .IsRequired();

            builder
                .Property(o => o.OrderDate)
                .IsRequired();

            builder
                .Property(o => o.OrderCancellationDate)
                .IsRequired(false);

            builder.OwnsOne(o => o.Address);
            // to change value object Address property names from Address_Street
            // use o.Property(p => p.Street).HasColumnName("ShipsToStreet");

            builder.OwnsOne(o => o.PaymentData);

            // If its set to IsRequired EF will use cascade deleting by default
            builder.HasMany(o => o.OrderItems)
                .WithOne()
                .IsRequired();

            var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderItems));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
