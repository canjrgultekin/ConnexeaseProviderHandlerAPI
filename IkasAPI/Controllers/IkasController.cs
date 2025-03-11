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

        [HttpPost("get-customer")]
        public async Task<IActionResult> GetCustomer([FromBody] IkasRequestDto request)
        {
            var customerData = await _ikasService.GetCustomerDataAsync(request.ProjectName, request.CustomerId, request.AuthToken);
            return Ok(customerData);
        }
    }
}
