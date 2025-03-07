using System.Threading.Tasks;
using ProviderHandlerAPI.Models;

namespace ProviderHandlerAPI.Services.Ikas
{
    public interface IIkasApiClient
    {
        Task<object> GetCustomerDataAsync(ClientRequestDto request);
        Task<object> SendRequestToIkasAsync(ClientRequestDto request);
    }
}
