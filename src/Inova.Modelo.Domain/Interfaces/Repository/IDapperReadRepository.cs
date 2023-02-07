using System.Threading.Tasks;

namespace Inova.Modelo.Domain.Interfaces.Repository;

public interface IDapperReadRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetByIdAsync(int id);
}
