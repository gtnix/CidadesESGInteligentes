namespace GestaoResiduosESG.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<(IEnumerable<T> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
