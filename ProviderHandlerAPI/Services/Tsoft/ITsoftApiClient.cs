using System.Threading.Tasks;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Models.Tsoft;

namespace ProviderHandlerAPI.Services.Tsoft
{
    public interface ITsoftApiClient
    {
        Task<object> GetCustomerDataAsync(ClientRequestDto request);
        Task<object> SendRequestToTsoftAsync(ClientRequestDto request);
    }
}
