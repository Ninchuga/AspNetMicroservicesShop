using AutoMapper;
using EventBus.Messages.Events.Order;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<Order, CheckoutOrderCommand>().ReverseMap();
            CreateMap<Order, PlaceOrderCommand>().ReverseMap();
            CreateMap<Order, UpdateOrderCommand>().ReverseMap();
            CreateMap<OrderStatusUpdated, UpdateOrderStatusCommand>();
                //.ForMember(destination => destination.OrderStatus, op => op.MapFrom(source => source.OrderStatus));
        }
    }
}
