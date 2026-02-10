using ITAssetManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ITAssetManagement.Repo.Interfaces;

namespace ITAssetManagement.Repo.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        internal DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // --- CÁC HÀM CŨ GIỮ NGUYÊN ---
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void Delete(T entity) => _dbSet.Remove(entity);
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public void Update(T entity) => _dbSet.Update(entity);

        // --- TRIỂN KHAI HÀM MỚI ---
        public IQueryable<T> GetAll()
        {
            // Trả về dạng truy vấn để nối bảng (Join)
            return _dbSet.AsQueryable();
        }
    }
}