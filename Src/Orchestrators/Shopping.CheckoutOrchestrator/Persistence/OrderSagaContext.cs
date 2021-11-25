using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shopping.CheckoutOrchestrator.Models;
using System;

namespace Shopping.CheckoutOrchestrator.Persistence
{
    public class OrderSagaContext : DbContext
    {

        public OrderSagaContext(DbContextOptions<OrderSagaContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(b => b.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Order>()
                .Property(order => order.OrderState)
                .HasConversion<string>();
        }

        public DbSet<Order> Orders { get; set; }
    }
}
