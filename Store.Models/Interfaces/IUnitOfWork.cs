using Store.Models.Interfaces.Repositories;

namespace Store.Models.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
    Task ExecuteStrategyAsync(Func<Task> operation);
    Task<T> ExecuteStrategyAsync<T>(Func<Task<T>> operation);
}
