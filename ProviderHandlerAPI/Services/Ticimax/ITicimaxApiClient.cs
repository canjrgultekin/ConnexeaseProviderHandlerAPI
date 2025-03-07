using System.Threading.Tasks;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Models.Ticimax;

namespace ProviderHandlerAPI.Services.Ticimax
{
    public interface ITicimaxApiClient
    {
        Task<object> GetCustomerDataAsync(ClientRequestDto request);
        Task<TicimaxResponseDto> SendRequestToTicimaxAsync(ClientRequestDto request);
    }
}
