using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Repo.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Lấy tất cả bản ghi
        Task<IEnumerable<T>> GetAllAsync();

        // Lấy 1 bản ghi theo ID
        Task<T?> GetByIdAsync(int id);

        // Tìm kiếm linh hoạt (ví dụ: tìm theo tên, trạng thái...)
        // Cách dùng: repository.FindAsync(x => x.Name == "ABC");
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Thêm mới
        Task AddAsync(T entity);

        // Cập nhật
        void Update(T entity);

        // Xóa
        void Delete(T entity);
    }
}