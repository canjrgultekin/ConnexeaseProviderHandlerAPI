using System.Threading.Tasks;
using TicimaxAPI.Models;

namespace TicimaxAPI.Repositories
{
    public interface ITicimaxRepository
    {
        Task<object> GetTicimaxDataAsync(TicimaxRequestDto request);
    }
}
