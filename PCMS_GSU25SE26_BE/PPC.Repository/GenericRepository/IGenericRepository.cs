using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.GenericRepository
{
    public interface IGenericRepository<T> where T : class
    {
        List<T> GetAll();
        Task<List<T>> GetAllAsync();

        void Create(T entity);
        Task<T> CreateAsync(T entity);

        Task<int> CreateAsyncNoRequest(T entity);
        void Update(T entity);
        Task<int> UpdateAsync(T entity);

        bool Remove(T entity);
        Task<bool> RemoveAsync(T entity);

        T GetById(string code);
        Task<T> GetByIdAsync(string code);

        void PrepareCreate(T entity);
        void PrepareUpdate(T entity);
        void PrepareRemove(T entity);

        int Save();
        Task<int> SaveAsync();
    }
}
