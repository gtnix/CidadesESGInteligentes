using GestaoResiduosESG.Domain.Entities;

namespace GestaoResiduosESG.Domain.Interfaces;

public interface ILimiteArmazenamentoRepository : IRepository<LimiteArmazenamento>
{
    Task<LimiteArmazenamento?> GetByPontoEResiduoAsync(int pontoColetaId, int residuoId);
}
