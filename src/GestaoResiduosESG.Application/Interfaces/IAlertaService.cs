using GestaoResiduosESG.Application.ViewModels.Alerta;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Enums;

namespace GestaoResiduosESG.Application.Interfaces;

public interface IAlertaService
{
    Task<PagedResponseViewModel<AlertaResponseViewModel>> GetAllAsync(StatusAlerta? status, int page, int pageSize);
    Task<AlertaResponseViewModel?> GetByIdAsync(int id);
    Task<AlertaResponseViewModel?> ResolverAsync(int id, AlertaResolverViewModel vm);
}
