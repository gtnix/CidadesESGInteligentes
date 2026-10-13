using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.PontoResiduo;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;

namespace GestaoResiduosESG.Application.Services;

public class PontoResiduoService : IPontoResiduoService
{
    private readonly IPontoResiduoRepository _pontoResiduoRepository;
    private readonly IPontoColetaRepository _pontoColetaRepository;
    private readonly IResiduoRepository _residuoRepository;

    public PontoResiduoService(
        IPontoResiduoRepository pontoResiduoRepository,
        IPontoColetaRepository pontoColetaRepository,
        IResiduoRepository residuoRepository)
    {
        _pontoResiduoRepository = pontoResiduoRepository;
        _pontoColetaRepository = pontoColetaRepository;
        _residuoRepository = residuoRepository;
    }

    public async Task<PagedResponseViewModel<PontoResiduoResponseViewModel>> GetByPontoColetaAsync(int pontoColetaId, int page, int pageSize)
    {
        var ponto = await _pontoColetaRepository.GetByIdAsync(pontoColetaId)
            ?? throw new NotFoundException($"Ponto de coleta {pontoColetaId} não encontrado");

        var todos = await _pontoResiduoRepository.GetByPontoColetaAsync(pontoColetaId);

        var vinculados = todos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pr => new PontoResiduoResponseViewModel
            {
                PontoColetaId = pr.PontoColetaId,
                ResiduoId = pr.ResiduoId,
                NomeResiduo = pr.Residuo?.Nome ?? string.Empty,
                CategoriaResiduo = pr.Residuo?.Categoria.ToString() ?? string.Empty,
                UnidadeMedida = pr.Residuo?.UnidadeMedida ?? string.Empty
            });

        return new PagedResponseViewModel<PontoResiduoResponseViewModel>
        {
            Data = vinculados,
            Page = page,
            PageSize = pageSize,
            TotalCount = todos.Count()
        };
    }

    public async Task VincularAsync(int pontoColetaId, int residuoId)
    {
        var ponto = await _pontoColetaRepository.GetByIdAsync(pontoColetaId)
            ?? throw new NotFoundException($"Ponto de coleta {pontoColetaId} não encontrado");

        var residuo = await _residuoRepository.GetByIdAsync(residuoId)
            ?? throw new NotFoundException($"Resíduo {residuoId} não encontrado");

        if (await _pontoResiduoRepository.ExisteAsync(pontoColetaId, residuoId))
            throw new BusinessException($"O resíduo '{residuo.Nome}' já está vinculado ao ponto '{ponto.Nome}'");

        await _pontoResiduoRepository.AddAsync(new PontoResiduo
        {
            PontoColetaId = pontoColetaId,
            ResiduoId = residuoId
        });
    }

    public async Task DesvincularAsync(int pontoColetaId, int residuoId)
    {
        if (!await _pontoResiduoRepository.ExisteAsync(pontoColetaId, residuoId))
            throw new NotFoundException($"Vínculo entre ponto {pontoColetaId} e resíduo {residuoId} não encontrado");

        await _pontoResiduoRepository.DeleteAsync(pontoColetaId, residuoId);
    }
}
