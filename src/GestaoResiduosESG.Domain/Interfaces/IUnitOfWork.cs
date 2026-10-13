namespace GestaoResiduosESG.Domain.Interfaces;

/// <summary>Abstração de transação para garantir atomicidade em operações multi-repositório</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
