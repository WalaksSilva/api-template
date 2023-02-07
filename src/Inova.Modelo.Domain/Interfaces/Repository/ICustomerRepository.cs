using System.Collections.Generic;
using System.Threading.Tasks;
using Inova.Modelo.Domain.Models;
using Inova.Modelo.Domain.Models.Dapper;

namespace Inova.Modelo.Domain.Interfaces.Repository;

public interface ICustomerRepository : IEntityBaseRepository<Customer>, IDapperReadRepository<Customer>
{
    Task<IEnumerable<CustomerAddress>> GetAllAsync();
    Task<CustomerAddress> GetAddressByIdAsync(int id);
    Task<CustomerAddress> GetByNameAsync(string name);
}
