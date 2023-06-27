using Aviask.Models;

namespace Aviask.Repositories
{
    public interface IAviaskRepository<T> where T : BaseModel
    {
        public IQueryable<T> GetAll();
        public Task CreateAsync(T obj);

        public Task UpdateAsync(T obj);

        public Task DeleteAsync(T obj);
        public Task DeleteFromIdAsync(int id);

        public Task<T?> GetByIdAsync(int id);

        public Task<bool> ExistsByIdAsync(int id);
    }
}
