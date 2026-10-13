using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Repositories;

public class LimiteArmazenamentoRepository : Repository<LimiteArmazenamento>, ILimiteArmazenamentoRepository
{
    public LimiteArmazenamentoRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<(IEnumerable<LimiteArmazenamento> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.LimitesArmazenamento
            .Include(l => l.PontoColeta)
            .Include(l => l.Residuo)
            .OrderBy(l => l.PontoColeta.Nome);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public override async Task<LimiteArmazenamento?> GetByIdAsync(int id)
        => await _context.LimitesArmazenamento
            .Include(l => l.PontoColeta)
            .Include(l => l.Residuo)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<LimiteArmazenamento?> GetByPontoEResiduoAsync(int pontoColetaId, int residuoId)
        => await _context.LimitesArmazenamento
            .FirstOrDefaultAsync(l => l.PontoColetaId == pontoColetaId && l.ResiduoId == residuoId);
}
