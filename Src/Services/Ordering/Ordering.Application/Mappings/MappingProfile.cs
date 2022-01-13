using AutoMapper;
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
                .ForMember(destination => destination.FirstName, op => op.MapFrom(source => source.Address.FirstName))
                .ForMember(destination => destination.LastName, op => op.MapFrom(source => source.Address.LastName))
                .ForMember(destination => destination.Email, op => op.MapFrom(source => source.Address.Email))
                .ForMember(destination => destination.Street, op => op.MapFrom(source => source.Address.Street))
                .ForMember(destination => destination.Country, op => op.MapFrom(source => source.Address.Country))
                .ForMember(destination => destination.City, op => op.MapFrom(source => source.Address.City))
                .ForMember(destination => destination.CardName, op => op.MapFrom(source => source.PaymentData.CardName))
                .ForMember(destination => destination.CardNumber, op => op.MapFrom(source => source.PaymentData.CardNumber))
                .ForMember(destination => destination.OrderPaid, op => op.MapFrom(source => source.PaymentData.OrderPaid))
                .ReverseMap();

            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
        }
    }
}
