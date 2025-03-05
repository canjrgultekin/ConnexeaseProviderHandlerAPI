using Microsoft.AspNetCore.Mvc;
using TicimaxAPI.Services;
using TicimaxAPI.Models;

namespace TicimaxAPI.Controllers
{
    [ApiController]
    [Route("api/ticimax")]
    public class TicimaxController : ControllerBase
    {
        private readonly ITicimaxService _ticimaxService;

        public TicimaxController(ITicimaxService ticimaxService)
        {
            _ticimaxService = ticimaxService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessRequest([FromBody] TicimaxRequestDto request)
        {
            var response = await _ticimaxService.HandleTicimaxRequestAsync(request);
            return Ok(response);
        }

        [HttpGet("get-customer")]
        public async Task<IActionResult> GetCustomer([FromQuery] string customerId)
        {
            var customerData = await _ticimaxService.GetCustomerDataAsync(customerId);
            return Ok(customerData);
        }
    }
}
