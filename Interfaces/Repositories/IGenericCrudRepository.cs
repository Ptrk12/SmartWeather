namespace Interfaces.Repositories
{
    public interface IGenericCrudRepository<T> where T : class
    {
        Task<bool> AddAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<T?> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>>? includeFunc = null);
    }
}
