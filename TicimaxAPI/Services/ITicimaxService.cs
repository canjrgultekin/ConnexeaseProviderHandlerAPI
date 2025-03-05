using System.Threading.Tasks;
using TicimaxAPI.Models;

namespace TicimaxAPI.Services
{
    public interface ITicimaxService
    {
        Task<TicimaxResponseDto> HandleTicimaxRequestAsync(TicimaxRequestDto request);
        Task<object> GetCustomerDataAsync(string projectName,string sessionId, string customerId);
    }
}
