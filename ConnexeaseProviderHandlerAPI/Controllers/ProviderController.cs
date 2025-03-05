using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;
using ConnexeaseProviderHandlerAPI.Services;

namespace ConnexeaseProviderHandlerAPI.Controllers
{
    [ApiController]
    [Route("api/provider")]
    public class ProviderController : ControllerBase
    {
        private readonly ProviderHandler _providerHandler;

        public ProviderController(ProviderHandler providerHandler)
        {
            _providerHandler = providerHandler;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessClientData([FromBody] ClientRequestDto request)
        {
            var response = await _providerHandler.HandleRequestAsync(request);
            return Ok(response);
        }
    }
}
