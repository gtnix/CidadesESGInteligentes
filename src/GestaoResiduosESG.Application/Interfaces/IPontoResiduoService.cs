using GestaoResiduosESG.Application.ViewModels.PontoResiduo;
using GestaoResiduosESG.Application.ViewModels.Shared;

namespace GestaoResiduosESG.Application.Interfaces;

public interface IPontoResiduoService
{
    Task<PagedResponseViewModel<PontoResiduoResponseViewModel>> GetByPontoColetaAsync(int pontoColetaId, int page, int pageSize);
    Task VincularAsync(int pontoColetaId, int residuoId);
    Task DesvincularAsync(int pontoColetaId, int residuoId);
}
