using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public interface ITicimaxApiClient
    {
        Task<TicimaxResponseDto> SendRequestToTicimaxAsync(ClientRequestDto request);
    }
}
