using FluentValidation;
using Inova.Modelo.Domain.Models;

namespace Inova.Modelo.Domain.Validation.CustomerValidation;

public class CustomerDeleteValidation : AbstractValidator<Customer>
{
    public CustomerDeleteValidation()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Id não pode ser nulo");
    }
}
