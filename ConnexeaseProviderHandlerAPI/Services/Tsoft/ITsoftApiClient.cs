using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;
using ConnexeaseProviderHandlerAPI.Models.Tsoft;

namespace ConnexeaseProviderHandlerAPI.Services.Tsoft
{
    public interface ITsoftApiClient
    {
        Task<TsoftCustomerResponseDto> GetCustomerDataAsync(ClientRequestDto request);
        Task<TsoftResponseDto> SendRequestToTsoftAsync(ClientRequestDto request);
    }
}
