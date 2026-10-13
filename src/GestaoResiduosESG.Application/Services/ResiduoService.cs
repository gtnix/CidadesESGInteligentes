using AutoMapper;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;

namespace GestaoResiduosESG.Application.Services;

public class ResiduoService : IResiduoService
{
    private readonly IResiduoRepository _repository;
    private readonly IMapper _mapper;

    public ResiduoService(IResiduoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResponseViewModel<ResiduoResponseViewModel>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllPagedAsync(page, pageSize);
        return new PagedResponseViewModel<ResiduoResponseViewModel>
        {
            Data = _mapper.Map<IEnumerable<ResiduoResponseViewModel>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ResiduoResponseViewModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<ResiduoResponseViewModel>(entity);
    }

    public async Task<ResiduoResponseViewModel> CreateAsync(ResiduoCreateViewModel vm)
    {
        var entity = _mapper.Map<Residuo>(vm);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<ResiduoResponseViewModel>(created);
    }

    public async Task<ResiduoResponseViewModel?> UpdateAsync(int id, ResiduoUpdateViewModel vm)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        _mapper.Map(vm, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<ResiduoResponseViewModel>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }
}
