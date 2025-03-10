using System.Threading.Tasks;

namespace IkasAPI.Services
{
    public interface IIkasAdminService
    {
        Task<object> ListCustomers(string projectName);
        Task<object> ListProducts(string projectName, string productId = null, string brandId = null);
        Task<object> ListOrders(string projectName, string orderId = null, string customerId = null, string customerEmail = null);
    }
}
