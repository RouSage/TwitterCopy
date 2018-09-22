using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        Task SaveAsync();
    }
}
