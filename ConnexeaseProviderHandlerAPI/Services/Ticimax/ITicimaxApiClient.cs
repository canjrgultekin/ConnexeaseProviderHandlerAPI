using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;
using ConnexeaseProviderHandlerAPI.Models.Ticimax;

namespace ConnexeaseProviderHandlerAPI.Services.Ticimax
{
    public interface ITicimaxApiClient
    {
        Task<object> GetCustomerDataAsync(ClientRequestDto request);
        Task<TicimaxResponseDto> SendRequestToTicimaxAsync(ClientRequestDto request);
    }
}
