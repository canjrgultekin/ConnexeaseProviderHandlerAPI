using System.Threading.Tasks;
using IkasAPI.Models;

namespace IkasAPI.Services
{
    public interface IIkasService
    {
        Task<object> HandleIkasRequestAsync(IkasRequestDto request);
        Task<object> GetCustomerDataAsync(string projectName,string customerId, string authToken);
    }
}
