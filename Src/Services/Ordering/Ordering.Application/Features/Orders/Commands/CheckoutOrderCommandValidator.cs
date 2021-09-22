using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands
{
    public class CheckoutOrderCommandValidator : AbstractValidator<CheckoutOrderCommand>
    {
        public CheckoutOrderCommandValidator()
        {
            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("{FirstName} is required.")
                .NotNull();

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("{LastName} is required.")
                .NotNull();

            RuleFor(p => p.Address)
                .NotEmpty().WithMessage("{Address} is required.")
                .NotNull();

            RuleFor(p => p.Email)
               .NotEmpty().WithMessage("{EmailAddress} is required.");

            RuleFor(p => p.TotalPrice)
                .NotEmpty().WithMessage("{TotalPrice} is required.")
                .GreaterThan(0).WithMessage("{TotalPrice} should be greater than zero.");

            RuleFor(p => p.CardName)
                .NotEmpty().WithMessage("{CardName} is required.")
                .NotNull();

            RuleFor(p => p.CardNumber)
                .NotEmpty().WithMessage("{CardNumber} is required.")
                .NotNull();
        }
    }
}
