using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ITAssetManagement.Repo.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // --- GIỮ LẠI CÁC HÀM CŨ ---
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        // --- THÊM MỚI HÀM NÀY ĐỂ SỬA LỖI ---
        // Hàm này trả về IQueryable để bên Service có thể .Include() (nối bảng)
        IQueryable<T> GetAll();
        // -----------------------------------
    }
}