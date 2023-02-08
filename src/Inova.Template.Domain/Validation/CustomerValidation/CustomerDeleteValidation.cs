using FluentValidation;
using Inova.Template.Domain.Models;

namespace Inova.Template.Domain.Validation.CustomerValidation;

public class CustomerDeleteValidation : AbstractValidator<Customer>
{
    public CustomerDeleteValidation()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Id não pode ser nulo");
    }
}
