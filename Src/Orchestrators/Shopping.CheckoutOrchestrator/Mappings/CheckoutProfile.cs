using AutoMapper;
using EventBus.Messages.Checkout;
using Shopping.CheckoutOrchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.CheckoutOrchestrator.Mappings
{
    public class CheckoutProfile : Profile
    {
        public CheckoutProfile()
        {
            CreateMap<Checkout, OrderStatusUpdated>();
        }
    }
}
