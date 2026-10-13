using GestaoResiduosESG.Domain.Entities;

namespace GestaoResiduosESG.Domain.Interfaces;

public interface IPontoResiduoRepository
{
    Task<bool> ExisteAsync(int pontoColetaId, int residuoId);
    Task<IEnumerable<PontoResiduo>> GetByPontoColetaAsync(int pontoColetaId);
    Task AddAsync(PontoResiduo pontoResiduo);
    Task DeleteAsync(int pontoColetaId, int residuoId);
}
