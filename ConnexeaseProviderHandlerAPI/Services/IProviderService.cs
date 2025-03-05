using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public interface IProviderService
    {
        Task<string> ProcessRequestAsync(ClientRequestDto request);
    }
}
