using AutoMapper;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;

namespace GestaoResiduosESG.Application.Services;

public class PontoColetaService : IPontoColetaService
{
    private readonly IPontoColetaRepository _repository;
    private readonly IAlertaRepository _alertaRepository;
    private readonly ILimiteArmazenamentoRepository _limiteRepository;
    private readonly IPontoResiduoRepository _pontoResiduoRepository;
    private readonly IMapper _mapper;

    public PontoColetaService(
        IPontoColetaRepository repository,
        IAlertaRepository alertaRepository,
        ILimiteArmazenamentoRepository limiteRepository,
        IPontoResiduoRepository pontoResiduoRepository,
        IMapper mapper)
    {
        _repository = repository;
        _alertaRepository = alertaRepository;
        _limiteRepository = limiteRepository;
        _pontoResiduoRepository = pontoResiduoRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponseViewModel<PontoColetaResponseViewModel>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllPagedAsync(page, pageSize);
        return new PagedResponseViewModel<PontoColetaResponseViewModel>
        {
            Data = _mapper.Map<IEnumerable<PontoColetaResponseViewModel>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<PontoColetaResponseViewModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<PontoColetaResponseViewModel>(entity);
    }

    public async Task<PontoColetaStatusViewModel?> GetStatusAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        var percentual = entity.CapacidadeMaxima > 0
            ? Math.Round((entity.QuantidadeAtual / entity.CapacidadeMaxima) * 100m, 2)
            : 0m;

        var statusOcupacao = percentual switch
        {
            >= 100m => StatusOcupacao.Cheio,
            >= 90m  => StatusOcupacao.Critico,
            >= 80m  => StatusOcupacao.Alerta,
            _       => StatusOcupacao.Normal
        };

        var alertasPendentes = await _alertaRepository.GetByPontoColetaAsync(id);

        return new PontoColetaStatusViewModel
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Localizacao = entity.Localizacao,
            CapacidadeMaxima = entity.CapacidadeMaxima,
            QuantidadeAtual = entity.QuantidadeAtual,
            PercentualOcupacao = percentual,
            StatusOcupacao = statusOcupacao.ToString(),
            TotalAlertasPendentes = alertasPendentes.Count()
        };
    }

    public async Task<PontoColetaResponseViewModel> CreateAsync(PontoColetaCreateViewModel vm)
    {
        var entity = _mapper.Map<PontoColeta>(vm);
        entity.QuantidadeAtual = 0;
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<PontoColetaResponseViewModel>(created);
    }

    public async Task<PontoColetaResponseViewModel?> UpdateAsync(int id, PontoColetaUpdateViewModel vm)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        var quantidadeAtual = entity.QuantidadeAtual;
        _mapper.Map(vm, entity);
        entity.QuantidadeAtual = quantidadeAtual;
        await _repository.UpdateAsync(entity);
        return _mapper.Map<PontoColetaResponseViewModel>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }

    public async Task<bool> RegistrarColetaAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;

        // Zera quantidade (= SP_REGISTRAR_COLETA)
        entity.QuantidadeAtual = 0;
        await _repository.UpdateAsync(entity);

        // Resolve todos os alertas pendentes do ponto
        var alertasPendentes = await _alertaRepository.GetByPontoColetaAsync(id);
        foreach (var alerta in alertasPendentes)
        {
            alerta.Status = StatusAlerta.Resolvido;
            alerta.Mensagem += " | Resolvido automaticamente por coleta registrada";
            await _alertaRepository.UpdateAsync(alerta);
        }

        // Verifica se a quantidade zerada ficou abaixo do LimiteMinimo configurado
        // e gera alerta TipoAlerta.LimiteMinimo para cada resíduo vinculado que tenha limite configurado
        var residuosVinculados = await _pontoResiduoRepository.GetByPontoColetaAsync(id);
        foreach (var vinculo in residuosVinculados)
        {
            var limite = await _limiteRepository.GetByPontoEResiduoAsync(id, vinculo.ResiduoId);
            if (limite is not null && entity.QuantidadeAtual < limite.LimiteMinimo)
            {
                if (!await _alertaRepository.ExisteAlertaPendenteAsync(id, TipoAlerta.LimiteMinimo))
                {
                    await _alertaRepository.AddAsync(new Alerta
                    {
                        PontoColetaId = id,
                        ResiduoId = vinculo.ResiduoId,
                        Tipo = TipoAlerta.LimiteMinimo,
                        Mensagem = $"Quantidade atual ({entity.QuantidadeAtual} kg) está abaixo do limite mínimo configurado ({limite.LimiteMinimo} kg) no ponto '{entity.Nome}'",
                        Status = StatusAlerta.Pendente,
                        DataAlerta = DateTime.Now
                    });
                }
            }
        }

        return true;
    }
}
