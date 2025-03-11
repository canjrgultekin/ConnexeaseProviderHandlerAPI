using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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

        [HttpPost("get-customer")]
        public async Task<IActionResult> GetCustomer([FromBody] TicimaxRequestDto request)
        {
            var customerData = await _ticimaxService.GetCustomerDataAsync(request.ProjectName,request.SessionId, request.CustomerId);
            return Ok(customerData);
        }
    }
}
