using System.Threading.Tasks;
using Inova.Modelo.Domain.Models.Services;

namespace Inova.Modelo.Domain.Interfaces.Services;

public interface IViaCEPService
{
    Task<ViaCEP> GetByCEPAsync(string cep);
}
