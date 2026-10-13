using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Repositories;

/// <summary>
/// Classe base genérica que implementa as operações comuns de CRUD para todos os repositórios.
/// Elimina a duplicação de AddAsync, UpdateAsync e DeleteAsync em cada repositório concreto.
/// Os repositórios filhos devem implementar apenas GetAllPagedAsync (ordenação/includes variam por entidade)
/// e podem sobrescrever GetByIdAsync quando precisam de eager loading (Include).
/// </summary>
public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();
    }

    public abstract Task<(IEnumerable<T> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<T> AddAsync(T entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
