using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using GestaoResiduosESG.Application.ViewModels.Shared;

namespace GestaoResiduosESG.Application.Interfaces;

public interface IPontoColetaService
{
    Task<PagedResponseViewModel<PontoColetaResponseViewModel>> GetAllAsync(int page, int pageSize);
    Task<PontoColetaResponseViewModel?> GetByIdAsync(int id);
    Task<PontoColetaStatusViewModel?> GetStatusAsync(int id);
    Task<PontoColetaResponseViewModel> CreateAsync(PontoColetaCreateViewModel vm);
    Task<PontoColetaResponseViewModel?> UpdateAsync(int id, PontoColetaUpdateViewModel vm);
    Task<bool> DeleteAsync(int id);
    Task<bool> RegistrarColetaAsync(int id);
}
