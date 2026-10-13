using GestaoResiduosESG.Application.ViewModels.Descarte;
using GestaoResiduosESG.Application.ViewModels.Shared;

namespace GestaoResiduosESG.Application.Interfaces;

public interface IDescarteService
{
    Task<PagedResponseViewModel<DescarteResponseViewModel>> GetAllAsync(int page, int pageSize);
    Task<DescarteResponseViewModel?> GetByIdAsync(int id);
    Task<DescarteResponseViewModel> CreateAsync(DescarteCreateViewModel vm);
    Task<bool> DeleteAsync(int id);
}
