using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Repositories;

public class DescarteRepository : Repository<Descarte>, IDescarteRepository
{
    public DescarteRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<(IEnumerable<Descarte> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.Descartes
            .Include(d => d.Residuo)
            .Include(d => d.PontoColeta)
            .OrderByDescending(d => d.DataDescarte);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public override async Task<Descarte?> GetByIdAsync(int id)
        => await _context.Descartes
            .Include(d => d.Residuo)
            .Include(d => d.PontoColeta)
            .FirstOrDefaultAsync(d => d.Id == id);
}
