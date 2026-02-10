using ITAssetManagement.Request.User; // 👈 QUAN TRỌNG: Trỏ vào nơi chứa LoginRequest/Response
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}