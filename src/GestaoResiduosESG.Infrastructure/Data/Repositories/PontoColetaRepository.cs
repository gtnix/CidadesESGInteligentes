using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Repositories;

public class PontoColetaRepository : Repository<PontoColeta>, IPontoColetaRepository
{
    public PontoColetaRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<(IEnumerable<PontoColeta> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.PontosColeta.OrderBy(p => p.Nome);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }
}
