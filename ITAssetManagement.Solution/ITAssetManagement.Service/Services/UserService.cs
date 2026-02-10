using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Service.Interfaces;

namespace ITAssetManagement.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.GetRepository<User>().GetAllAsync();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            // Logic: Kiểm tra xem user đã tồn tại chưa
            var existingUsers = await _unitOfWork.GetRepository<User>().GetAllAsync();
            if (existingUsers.Any(u => u.Username == user.Username))
            {
                return false; // Đã tồn tại
            }

            user.CreatedDate = DateTime.Now;
            // Ở đây bạn có thể thêm logic mã hóa mật khẩu nếu muốn

            await _unitOfWork.GetRepository<User>().AddAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<User>();
            var user = await repo.GetByIdAsync(id);
            if (user == null) return false;

            repo.Delete(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            return users.FirstOrDefault(u => u.Username == username);
        }
    }
}