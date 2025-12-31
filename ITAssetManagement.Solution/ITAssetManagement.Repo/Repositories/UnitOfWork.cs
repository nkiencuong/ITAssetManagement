using ITAssetManagement.Models;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Repo.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITAssetManagement.Repo.Interfaces;

namespace ITAssetManagement.Repo.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private Hashtable _repositories; // Cache để lưu các Repository đã tạo

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CompleteAsync()
        {
            // Chỉ khi gọi hàm này, lệnh SQL mới thực sự chạy xuống Database
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            // Kiểm tra xem Repository này đã được tạo chưa, nếu chưa thì tạo mới
            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[type];
        }
    }
}