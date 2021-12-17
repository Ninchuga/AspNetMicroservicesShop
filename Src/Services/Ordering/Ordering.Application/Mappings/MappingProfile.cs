using AutoMapper;
using EventBus.Messages.Events.Order;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Helpers;
using Ordering.Application.Models;
using Ordering.Domain.Entities;

namespace Ordering.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(destination => destination.OrderId, op => op.MapFrom(source => source.Id))
                .ForMember(destination => destination.OrderStatus, op => op.MapFrom(source => source.OrderStatus.GetDescription()))
                .ReverseMap();
            CreateMap<Order, CheckoutOrderCommand>().ReverseMap();
            CreateMap<Order, PlaceOrderCommand>().ReverseMap();
            CreateMap<Order, UpdateOrderCommand>().ReverseMap();
            CreateMap<OrderStatusUpdated, UpdateOrderStatusCommand>();
                //.ForMember(destination => destination.OrderStatus, op => op.MapFrom(source => source.OrderStatus));
        }
    }
}
