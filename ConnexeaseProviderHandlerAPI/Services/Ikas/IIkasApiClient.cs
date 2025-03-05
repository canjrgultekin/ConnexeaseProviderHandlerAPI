using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services.Ikas
{
    public interface IIkasApiClient
    {
        Task<object> GetCustomerDataAsync(ClientRequestDto request);
        Task<object> SendRequestToIkasAsync(ClientRequestDto request);
    }
}
