namespace AviaskApi.Repositories;

public interface IAviaskRepository<T, TV>
{
    IQueryable<T> GetQuery();

    Task<T[]> GetAllAsync();

    Task CreateAsync(T entity);

    Task<T?> GetByIdAsync(TV id);

    Task<bool> ExistsByIdAsync(TV id);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);
}