using System.Threading.Tasks;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Models.Ticimax;

namespace ProviderHandlerAPI.Services.Ticimax
{
    public interface ITicimaxApiClient
    {
        Task<object> GetCustomerDataAsync(ClientRequestDto request);
        Task<object> SendRequestToTicimaxAsync(ClientRequestDto request);
    }
}
