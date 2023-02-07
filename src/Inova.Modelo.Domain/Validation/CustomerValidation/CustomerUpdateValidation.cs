﻿using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Inova.Modelo.Domain.Interfaces.Repository;
using Inova.Modelo.Domain.Models;

namespace Inova.Modelo.Domain.Validation.CustomerValidation;

public class CustomerUpdateValidation : AbstractValidator<Customer>
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerUpdateValidation(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;

        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Id não pode ser nulo");

        RuleFor(x => x.AddressId)
            .NotNull()
            .WithMessage("Endereço não pode ser nulo");

        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Nome não pode ser nulo");

        RuleFor(x => x)
            .MustAsync(ValidationName)
            .WithMessage("Nome já cadastrado na base de dados");
    }

    private async Task<bool> ValidationName(Customer customer, CancellationToken cancellationToken)
    {
        var customerRepository = await _customerRepository.GetByNameAsync(customer.Name);

        return customerRepository?.Id != customer.Id ? false : true;
    }
}