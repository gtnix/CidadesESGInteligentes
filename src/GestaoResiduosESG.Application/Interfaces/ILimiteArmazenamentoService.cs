using GestaoResiduosESG.Application.ViewModels.Limite;
using GestaoResiduosESG.Application.ViewModels.Shared;

namespace GestaoResiduosESG.Application.Interfaces;

public interface ILimiteArmazenamentoService
{
    Task<PagedResponseViewModel<LimiteResponseViewModel>> GetAllAsync(int page, int pageSize);
    Task<LimiteResponseViewModel?> GetByIdAsync(int id);
    Task<LimiteResponseViewModel> CreateAsync(LimiteCreateViewModel vm);
    Task<LimiteResponseViewModel?> UpdateAsync(int id, LimiteUpdateViewModel vm);
    Task<bool> DeleteAsync(int id);
}
