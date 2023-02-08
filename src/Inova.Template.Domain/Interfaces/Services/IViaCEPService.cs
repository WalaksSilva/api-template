using System.Threading.Tasks;
using Inova.Template.Domain.Models.Services;

namespace Inova.Template.Domain.Interfaces.Services;

public interface IViaCEPService
{
    Task<ViaCEP> GetByCEPAsync(string cep);
}
