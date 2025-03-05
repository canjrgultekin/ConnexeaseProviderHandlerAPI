using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public interface ITsoftApiClient
    {
        Task<object> GetCustomerData(string customerId);
        Task<TsoftResponseDto> SendRequestToTsoftAsync(ClientRequestDto request);
    }
}
