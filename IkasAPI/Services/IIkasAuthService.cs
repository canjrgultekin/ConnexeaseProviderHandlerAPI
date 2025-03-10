using System.Threading.Tasks;

namespace IkasAPI.Services
{
    public interface IIkasAuthService
    {
        Task<string> GetAuthAdminTokenAsync(string projectName);
    }
}
