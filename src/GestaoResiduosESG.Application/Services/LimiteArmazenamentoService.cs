using AutoMapper;
using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.Limite;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;

namespace GestaoResiduosESG.Application.Services;

public class LimiteArmazenamentoService : ILimiteArmazenamentoService
{
    private readonly ILimiteArmazenamentoRepository _repository;
    private readonly IPontoColetaRepository _pontoColetaRepository;
    private readonly IResiduoRepository _residuoRepository;
    private readonly IMapper _mapper;

    public LimiteArmazenamentoService(
        ILimiteArmazenamentoRepository repository,
        IPontoColetaRepository pontoColetaRepository,
        IResiduoRepository residuoRepository,
        IMapper mapper)
    {
        _repository = repository;
        _pontoColetaRepository = pontoColetaRepository;
        _residuoRepository = residuoRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponseViewModel<LimiteResponseViewModel>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllPagedAsync(page, pageSize);
        return new PagedResponseViewModel<LimiteResponseViewModel>
        {
            Data = _mapper.Map<IEnumerable<LimiteResponseViewModel>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<LimiteResponseViewModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<LimiteResponseViewModel>(entity);
    }

    public async Task<LimiteResponseViewModel> CreateAsync(LimiteCreateViewModel vm)
    {
        if (vm.LimiteMaximo <= vm.LimiteMinimo)
            throw new BusinessException("O limite máximo deve ser maior que o limite mínimo");

        var pontoExiste = await _pontoColetaRepository.GetByIdAsync(vm.PontoColetaId);
        if (pontoExiste is null)
            throw new BusinessException($"Ponto de coleta {vm.PontoColetaId} não encontrado");

        var residuoExiste = await _residuoRepository.GetByIdAsync(vm.ResiduoId);
        if (residuoExiste is null)
            throw new BusinessException($"Resíduo {vm.ResiduoId} não encontrado");

        var limiteExistente = await _repository.GetByPontoEResiduoAsync(vm.PontoColetaId, vm.ResiduoId);
        if (limiteExistente is not null)
            throw new BusinessException("Já existe um limite configurado para este par ponto/resíduo");

        var entity = _mapper.Map<LimiteArmazenamento>(vm);
        var created = await _repository.AddAsync(entity);

        var result = await _repository.GetByIdAsync(created.Id);
        return _mapper.Map<LimiteResponseViewModel>(result!);
    }

    public async Task<LimiteResponseViewModel?> UpdateAsync(int id, LimiteUpdateViewModel vm)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        if (vm.LimiteMaximo <= vm.LimiteMinimo)
            throw new BusinessException("O limite máximo deve ser maior que o limite mínimo");

        _mapper.Map(vm, entity);
        await _repository.UpdateAsync(entity);

        var result = await _repository.GetByIdAsync(id);
        return _mapper.Map<LimiteResponseViewModel>(result!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }
}
