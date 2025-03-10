using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IkasAPI.Services;

namespace IkasAPI.Controllers
{
    [ApiController]
    [Route("api/ikas/admin")]
    [ApiExplorerSettings(GroupName = "Ikas Admin")] // 🔥 Swagger sadece bunu gösterecek
    public class IkasAdminController : ControllerBase
    {
        private readonly IIkasAdminService _ikasAdminService;

        public IkasAdminController(IIkasAdminService ikasAdminService)
        {
            _ikasAdminService = ikasAdminService;
        }

        [HttpGet("list-customers")]
        public async Task<IActionResult> ListCustomers([FromQuery] string projectName)
        {
            var response = await _ikasAdminService.ListCustomers(projectName);
            return Ok(response);
        }

        [HttpGet("list-products")]
        public async Task<IActionResult> ListProducts([FromQuery] string projectName, [FromQuery] string productId = null, [FromQuery] string brandId = null)
        {
            var response = await _ikasAdminService.ListProducts(projectName, productId, brandId);
            return Ok(response);
        }

        [HttpGet("list-orders")]
        public async Task<IActionResult> ListOrders([FromQuery] string projectName, [FromQuery] string orderId = null, [FromQuery] string customerId = null, [FromQuery] string customerEmail = null)
        {
            var response = await _ikasAdminService.ListOrders(projectName, orderId, customerId, customerEmail);
            return Ok(response);
        }
    }
}
