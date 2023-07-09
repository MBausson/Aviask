using Aviask.Models;
using Microsoft.AspNetCore.Identity;

namespace Aviask.Repositories
{
    public interface IAviaskRepository<T, U> 
    {
        public IQueryable<T> GetAll();
        public Task CreateAsync(T obj);

        public Task UpdateAsync(T obj);

        public Task DeleteAsync(T obj);
        public Task DeleteFromIdAsync(U id);

        public Task<T?> GetByIdAsync(U id);

        public Task<bool> ExistsByIdAsync(U id);
    }
}
