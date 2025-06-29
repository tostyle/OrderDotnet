
using System.Linq.Expressions;

namespace Domain.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    // CRUD Operations
    Task<TEntity> GetByIdAsync(Guid id); // Assuming Guid as primary key
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);

    // Read Operations with Dynamic Filtering and Paging
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = null,
        int? skip = null,
        int? take = null);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

    // Maybe some raw SQL operations if needed, but try to avoid
    // IQueryable<TEntity> AsQueryable(); // If you want to expose IQueryable for complex scenarios
}