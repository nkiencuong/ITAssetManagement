using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITAssetManagement.Repo.Interfaces;

namespace ITAssetManagement.Repo.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Hàm này lấy ra Repository của một bảng bất kỳ (Asset, User, Supplier...)
        // Giúp bạn không cần khai báo từng Repository riêng lẻ
        IGenericRepository<T> GetRepository<T>() where T : class;

        // Lưu tất cả thay đổi xuống Database (Commit Transaction)
        Task<int> CompleteAsync();
        Task<int> CommitAsync();
    }
}