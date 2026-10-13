using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Repositories;

public class AlertaRepository : Repository<Alerta>, IAlertaRepository
{
    public AlertaRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<(IEnumerable<Alerta> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.Alertas
            .Include(a => a.PontoColeta)
            .Include(a => a.Residuo)
            .OrderByDescending(a => a.DataAlerta);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<(IEnumerable<Alerta> Items, int TotalCount)> GetAllPagedByStatusAsync(
        StatusAlerta? status, int page, int pageSize)
    {
        var query = _context.Alertas
            .Include(a => a.PontoColeta)
            .Include(a => a.Residuo)
            .Where(a => status == null || a.Status == status)
            .OrderByDescending(a => a.DataAlerta);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public override async Task<Alerta?> GetByIdAsync(int id)
        => await _context.Alertas
            .Include(a => a.PontoColeta)
            .Include(a => a.Residuo)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<bool> ExisteAlertaPendenteAsync(int pontoColetaId, TipoAlerta tipo)
        => await _context.Alertas.AnyAsync(a =>
            a.PontoColetaId == pontoColetaId &&
            a.Tipo == tipo &&
            (a.Status == StatusAlerta.Pendente || a.Status == StatusAlerta.EmAnalise));

    public async Task<IEnumerable<Alerta>> GetByPontoColetaAsync(int pontoColetaId)
        => await _context.Alertas
            .Where(a => a.PontoColetaId == pontoColetaId &&
                        (a.Status == StatusAlerta.Pendente || a.Status == StatusAlerta.EmAnalise))
            .ToListAsync();
}
