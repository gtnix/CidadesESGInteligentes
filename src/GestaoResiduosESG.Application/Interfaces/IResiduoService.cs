using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Application.ViewModels.Shared;

namespace GestaoResiduosESG.Application.Interfaces;

public interface IResiduoService
{
    Task<PagedResponseViewModel<ResiduoResponseViewModel>> GetAllAsync(int page, int pageSize);
    Task<ResiduoResponseViewModel?> GetByIdAsync(int id);
    Task<ResiduoResponseViewModel> CreateAsync(ResiduoCreateViewModel vm);
    Task<ResiduoResponseViewModel?> UpdateAsync(int id, ResiduoUpdateViewModel vm);
    Task<bool> DeleteAsync(int id);
}
