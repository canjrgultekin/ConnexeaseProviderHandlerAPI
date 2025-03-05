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

        [HttpGet("get-customer")]
        public async Task<IActionResult> GetCustomer([FromQuery] string projectName, [FromQuery] string customerId)
        {
            var customerData = await _tsoftService.GetCustomerDataAsync(projectName, customerId);
            return Ok(customerData);
        }
    }
}
