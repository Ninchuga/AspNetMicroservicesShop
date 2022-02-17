﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Entities;
using System;

namespace Ordering.Infrastructure.EntityConfigurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .ValueGeneratedNever()
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

            // If its set to IsRequired EF will use cascade deleting by default
            builder.HasMany(o => o.OrderItems)
                .WithOne()
                .IsRequired();

            var orderItemsNavigation = builder.Metadata.FindNavigation(nameof(Order.OrderItems));
            orderItemsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            // Configures a relationship where the Address is owned by (or part of) Order.
            // to change value object Address property names from Address_Street
            // use o.Property(p => p.Street).HasColumnName("ShipsToStreet");
            builder.OwnsOne(
                order => order.Address,
                    addressNavigationBuilder =>
                    {
                        // Configures a relationship where the Email is owned by (or part of) Addresses.
                        // In this case, is not used "ToTable();" to maintain the owned and owner in the same table. 
                        addressNavigationBuilder.OwnsOne(address => address.Email);
                    });

            // Configures a relationship where the PaymentData is owned by (or part of) Order.
            builder.OwnsOne(
                order => order.PaymentData,
                    paymentDataNavigationBuilder =>
                    {
                        // Configures a relationship where the Email is owned by (or part of) Addresses.
                        // In this case, is not used "ToTable();" to maintain the owned and owner in the same table. 
                        paymentDataNavigationBuilder.OwnsOne(address => address.CVV);
                    });
        }
    }
}
