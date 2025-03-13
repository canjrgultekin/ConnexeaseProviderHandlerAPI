using System.Threading.Tasks;
using TsoftAPI.Models;

namespace TsoftAPI.Services
{
    public interface ITsoftService
    {
        Task<object> HandleTsoftRequestAsync(TsoftRequestDto request);
        Task<object> GetCustomerDataAsync(TsoftRequestDto request); // 🔥 Eksik olan metod eklendi
    }
}
