using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;

namespace GestaoResiduosESG.Domain.Interfaces;

public interface IAlertaRepository : IRepository<Alerta>
{
    Task<bool> ExisteAlertaPendenteAsync(int pontoColetaId, TipoAlerta tipo);
    Task<IEnumerable<Alerta>> GetByPontoColetaAsync(int pontoColetaId);
    Task<(IEnumerable<Alerta> Items, int TotalCount)> GetAllPagedByStatusAsync(StatusAlerta? status, int page, int pageSize);
}
