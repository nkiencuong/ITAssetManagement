using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
    }
}