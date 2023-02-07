using AutoMapper;
using System.Diagnostics.CodeAnalysis;
using Inova.Modelo.API.ViewModels.Customer;
using Inova.Modelo.Domain.Models;
using Inova.Modelo.Domain.Models.Dapper;

namespace Inova.Modelo.API.AutoMapper;

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
