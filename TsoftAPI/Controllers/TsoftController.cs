using Microsoft.AspNetCore.Mvc;
using TsoftAPI.Services;
using TsoftAPI.Models;

namespace TsoftAPI.Controllers
{
    [ApiController]
    [Route("api/tsoft")]
    public class TsoftController : ControllerBase
    {
        private readonly ITsoftService _tsoftService;

        public TsoftController(ITsoftService tsoftService)
        {
            _tsoftService = tsoftService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessRequest([FromBody] TsoftRequestDto request)
        {
            var response = await _tsoftService.HandleTsoftRequestAsync(request);
            return Ok(response);
        }

        [HttpPost("get-customer")]
        public async Task<IActionResult> GetCustomer([FromBody] TsoftRequestDto request)
        {
            var response = await _tsoftService.GetCustomerDataAsync(request);
            return Ok(response);
        }

       
    }
}
