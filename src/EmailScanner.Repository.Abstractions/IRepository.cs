using EmailScanner.Domain.Abstractions;
using System.Linq.Expressions;

namespace EmailScanner.Repository.Abstractions;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void Remove(TEntity entity);
}
