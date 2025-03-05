using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TicimaxAPI.Models;

namespace TicimaxAPI.Repositories
{
    public class TicimaxRepository : ITicimaxRepository
    {
        private readonly HttpClient _httpClient;

        public TicimaxRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object> GetTicimaxDataAsync(TicimaxRequestDto request)
        {
            var response = await _httpClient.GetAsync($"https://api.ticimax.com/data/{request.CustomerId}");
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync());
        }
    }
}
