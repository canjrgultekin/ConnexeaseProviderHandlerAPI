using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IkasAPI.Services;
using IkasAPI.Models;

namespace IkasAPI.Controllers
{
    [ApiController]
    [Route("api/ikas")]
    public class IkasController : ControllerBase
    {
        private readonly IIkasService _ikasService;

        public IkasController(IIkasService ikasService)
        {
            _ikasService = ikasService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessRequest([FromBody] IkasRequestDto request)
        {
            var response = await _ikasService.HandleIkasRequestAsync(request);
            return Ok(response);
        }

        [HttpGet("get-customer")]
        public async Task<IActionResult> GetCustomer([FromQuery] string projectName, [FromQuery] string customerId, [FromQuery] string authToken)
        {
            var customerData = await _ikasService.GetCustomerDataAsync(projectName,customerId,authToken);
            return Ok(customerData);
        }
    }
}
