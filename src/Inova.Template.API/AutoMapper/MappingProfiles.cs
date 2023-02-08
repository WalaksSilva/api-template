using AutoMapper;
using System.Diagnostics.CodeAnalysis;
using Inova.Template.API.ViewModels.Customer;
using Inova.Template.Domain.Models;
using Inova.Template.Domain.Models.Dapper;

namespace Inova.Template.API.AutoMapper;

[ExcludeFromCodeCoverage]
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        #region Customer

        CreateMap<CustomerAddress, CustomerAddressViewModel>()
            .ConstructUsing(s => new CustomerAddressViewModel(
                s.Id,
                s.AddressId, 
                s.Name, 
                s.DateCreated, 
                s.CEP, 
                null));
        CreateMap<Customer, CustomerViewModel>()
            .ConstructUsing(s=> new CustomerViewModel(
                s.Id,
                s.AddressId,
                s.Name
            )).ReverseMap();

        #endregion
    }
}
