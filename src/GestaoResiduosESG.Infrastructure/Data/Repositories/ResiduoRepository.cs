using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Repositories;

public class ResiduoRepository : Repository<Residuo>, IResiduoRepository
{
    public ResiduoRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<(IEnumerable<Residuo> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.Residuos.OrderBy(r => r.Nome);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }
}
