using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Repositories;

public class PontoResiduoRepository : IPontoResiduoRepository
{
    private readonly ApplicationDbContext _context;

    public PontoResiduoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAsync(int pontoColetaId, int residuoId)
        => await _context.PontosResiduo
            .AnyAsync(pr => pr.PontoColetaId == pontoColetaId && pr.ResiduoId == residuoId);

    public async Task<IEnumerable<PontoResiduo>> GetByPontoColetaAsync(int pontoColetaId)
        => await _context.PontosResiduo
            .Include(pr => pr.Residuo)
            .Where(pr => pr.PontoColetaId == pontoColetaId)
            .ToListAsync();

    public async Task AddAsync(PontoResiduo pontoResiduo)
    {
        _context.PontosResiduo.Add(pontoResiduo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int pontoColetaId, int residuoId)
    {
        var entity = await _context.PontosResiduo
            .FindAsync(pontoColetaId, residuoId);
        if (entity != null)
        {
            _context.PontosResiduo.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
