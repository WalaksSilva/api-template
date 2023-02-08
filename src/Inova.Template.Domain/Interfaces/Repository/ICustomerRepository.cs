using System.Collections.Generic;
using System.Threading.Tasks;
using Inova.Template.Domain.Models;
using Inova.Template.Domain.Models.Dapper;

namespace Inova.Template.Domain.Interfaces.Repository;

public interface ICustomerRepository : IEntityBaseRepository<Customer>, IDapperReadRepository<Customer>
{
    Task<IEnumerable<CustomerAddress>> GetAllAsync();
    Task<CustomerAddress> GetAddressByIdAsync(int id);
    Task<CustomerAddress> GetByNameAsync(string name);
}
