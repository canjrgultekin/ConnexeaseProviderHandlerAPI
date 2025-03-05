using System.Threading.Tasks;
using TsoftAPI.Models;

namespace TsoftAPI.Services
{
    public interface ITsoftService
    {
        Task<TsoftResponseDto> HandleTsoftRequestAsync(TsoftRequestDto request);
        Task<TsoftCustomerResponseDto> GetCustomerDataAsync(string projectName, string customerId); // 🔥 Eksik olan metod eklendi
    }
}
