using System.Collections.Generic;
using System.Threading.Tasks;
using Inova.Modelo.API.ViewModels.Customer;

namespace Inova.Modelo.API.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerAddressViewModel>> GetAllAsync();
    Task<CustomerViewModel> GetByIdAsync(CustomerIdViewModel customerVM);
    Task<CustomerAddressViewModel> GetAddressByIdAsync(CustomerIdViewModel customerVM);
    Task<CustomerAddressViewModel> GetAddressByNameAsync(CustomerNameViewModel customerVM);
    Task RemoveAsync(CustomerViewModel customerVM);
    Task UpdateAsync(CustomerViewModel customerVM);
    Task<CustomerViewModel> AddAsync(CustomerViewModel customerVM);
}
