using System.Threading.Tasks;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Models.Tsoft;

namespace ProviderHandlerAPI.Services.Tsoft
{
    public interface ITsoftApiClient
    {
        Task<TsoftCustomerResponseDto> GetCustomerDataAsync(ClientRequestDto request);
        Task<TsoftResponseDto> SendRequestToTsoftAsync(ClientRequestDto request);
    }
}
