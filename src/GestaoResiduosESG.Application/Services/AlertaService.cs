using AutoMapper;
using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.Alerta;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;

namespace GestaoResiduosESG.Application.Services;

public class AlertaService : IAlertaService
{
    private readonly IAlertaRepository _repository;
    private readonly IMapper _mapper;

    public AlertaService(IAlertaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResponseViewModel<AlertaResponseViewModel>> GetAllAsync(StatusAlerta? status, int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllPagedByStatusAsync(status, page, pageSize);
        return new PagedResponseViewModel<AlertaResponseViewModel>
        {
            Data = _mapper.Map<IEnumerable<AlertaResponseViewModel>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AlertaResponseViewModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<AlertaResponseViewModel>(entity);
    }

    public async Task<AlertaResponseViewModel?> ResolverAsync(int id, AlertaResolverViewModel vm)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        if (entity.Status == StatusAlerta.Resolvido)
            throw new BusinessException("Este alerta já foi resolvido");

        if (entity.Status == StatusAlerta.Cancelado)
            throw new BusinessException("Não é possível resolver um alerta cancelado");

        entity.Status = StatusAlerta.Resolvido;

        if (!string.IsNullOrWhiteSpace(vm.Observacao))
            entity.Mensagem += $" | Resolução: {vm.Observacao}";

        await _repository.UpdateAsync(entity);
        return _mapper.Map<AlertaResponseViewModel>(entity);
    }

}
