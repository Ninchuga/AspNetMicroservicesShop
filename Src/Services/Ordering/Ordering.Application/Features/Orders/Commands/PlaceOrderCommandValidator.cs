using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(p => p.UserId)
                .NotEmpty().WithMessage("{UserId} is required");

            RuleFor(p => p.UserName)
                .NotEmpty().WithMessage("{UserName} is required.")
                .MaximumLength(50).WithMessage("{UserName} must not exceed 50 characters.");

            RuleFor(p => p.TotalPrice)
                .NotEmpty().WithMessage("{TotalPrice} is required.")
                .GreaterThan(0).WithMessage("{TotalPrice} should be greater than zero.");

            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("{FirstName} is required.")
                .MaximumLength(100).WithMessage("{FirstName} must not exceed 100 characters.");

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("{LastName} is required.")
                .MaximumLength(100).WithMessage("{LastName} must not exceed 100 characters.");

            RuleFor(p => p.Email)
               .NotEmpty().WithMessage("{Email} is required.")
               .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);

            RuleFor(p => p.Street)
                .NotEmpty().WithMessage("{Street} is required.")
                .MaximumLength(200).WithMessage("{Street} must not exceed 200 characters.");

            RuleFor(p => p.Country)
                .NotEmpty().WithMessage("{Country} is required.")
                .MaximumLength(50).WithMessage("{Country} must not exceed 50 characters.");

            RuleFor(p => p.City)
                .NotEmpty().WithMessage("{City} is required.")
                .MaximumLength(50).WithMessage("{City} must not exceed 50 characters.");

            RuleFor(p => p.CardName)
                .NotEmpty().WithMessage("{CardName} is required.")
                .MaximumLength(100).WithMessage("{CardName} must not exceed 100 characters.");

            RuleFor(p => p.CardNumber)
                .NotEmpty().WithMessage("{CardNumber} is required.")
                .MaximumLength(20).WithMessage("{CardNumber} must not exceed 20 characters.");

            RuleFor(p => p.CardExpiration)
                .NotEmpty().WithMessage("{CardExpiration} is required.")
                .MaximumLength(10).WithMessage("{CardExpiration} must not exceed 10 characters.");

            RuleFor(p => p.CVV)
                .NotEqual(0).WithMessage("{CVV} value must be greater than zero.")
                .Must(p => p.ToString().Length == 3).WithMessage("{CVV} must consist of 3 numbers.");
        }
    }
}
