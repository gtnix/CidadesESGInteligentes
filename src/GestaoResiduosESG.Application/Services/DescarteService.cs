using AutoMapper;
using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.Descarte;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;

namespace GestaoResiduosESG.Application.Services;

public class DescarteService : IDescarteService
{
    private readonly IDescarteRepository _descarteRepository;
    private readonly IPontoColetaRepository _pontoColetaRepository;
    private readonly IResiduoRepository _residuoRepository;
    private readonly ILimiteArmazenamentoRepository _limiteRepository;
    private readonly IAlertaRepository _alertaRepository;
    private readonly IPontoResiduoRepository _pontoResiduoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DescarteService(
        IDescarteRepository descarteRepository,
        IPontoColetaRepository pontoColetaRepository,
        IResiduoRepository residuoRepository,
        ILimiteArmazenamentoRepository limiteRepository,
        IAlertaRepository alertaRepository,
        IPontoResiduoRepository pontoResiduoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _descarteRepository = descarteRepository;
        _pontoColetaRepository = pontoColetaRepository;
        _residuoRepository = residuoRepository;
        _limiteRepository = limiteRepository;
        _alertaRepository = alertaRepository;
        _pontoResiduoRepository = pontoResiduoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponseViewModel<DescarteResponseViewModel>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _descarteRepository.GetAllPagedAsync(page, pageSize);
        return new PagedResponseViewModel<DescarteResponseViewModel>
        {
            Data = _mapper.Map<IEnumerable<DescarteResponseViewModel>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<DescarteResponseViewModel?> GetByIdAsync(int id)
    {
        var entity = await _descarteRepository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<DescarteResponseViewModel>(entity);
    }

    public async Task<DescarteResponseViewModel> CreateAsync(DescarteCreateViewModel vm)
    {
        // 1. Validar entidades referenciadas (fora da transação — apenas leitura)
        var ponto = await _pontoColetaRepository.GetByIdAsync(vm.PontoColetaId)
            ?? throw new NotFoundException($"Ponto de coleta {vm.PontoColetaId} não encontrado");

        var residuo = await _residuoRepository.GetByIdAsync(vm.ResiduoId)
            ?? throw new NotFoundException($"Resíduo {vm.ResiduoId} não encontrado");

        // 2. Validar se resíduo é aceito pelo ponto (= TRG_VALIDA_DESCARTE)
        if (!await _pontoResiduoRepository.ExisteAsync(vm.PontoColetaId, vm.ResiduoId))
            throw new BusinessException($"O resíduo '{residuo.Nome}' não é aceito pelo ponto de coleta '{ponto.Nome}'");

        // 3. Transação garante atomicidade: descarte + atualização do ponto + alertas são um bloco único
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 3a. Registrar descarte
            var descarte = _mapper.Map<Descarte>(vm);
            await _descarteRepository.AddAsync(descarte);

            // 3b. Atualizar quantidade no ponto (= TRG_ATUALIZA_QUANTIDADE)
            ponto.QuantidadeAtual += vm.Quantidade;
            await _pontoColetaRepository.UpdateAsync(ponto);

            // 3c. Verificar limites e gerar alertas (= TRG_VERIFICA_LIMITE_ALERTA + TRG_ALERTA_COLETA_NECESSARIA)
            await VerificarEGerarAlertasAsync(ponto, residuo);

            await _unitOfWork.CommitAsync();

            // Recarregar com navegações para resposta completa
            var created = await _descarteRepository.GetByIdAsync(descarte.Id);
            return _mapper.Map<DescarteResponseViewModel>(created!);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _descarteRepository.GetByIdAsync(id);
        if (entity is null) return false;

        // Transação garante atomicidade: atualização do ponto e deleção do descarte são um bloco único
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Reverte a quantidade no ponto ao excluir o descarte (consistência de estado)
            var ponto = await _pontoColetaRepository.GetByIdAsync(entity.PontoColetaId);
            if (ponto is not null)
            {
                ponto.QuantidadeAtual = Math.Max(0, ponto.QuantidadeAtual - entity.Quantidade);
                await _pontoColetaRepository.UpdateAsync(ponto);
            }

            await _descarteRepository.DeleteAsync(entity);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task VerificarEGerarAlertasAsync(PontoColeta ponto, Residuo residuo)
    {
        var percentual = ponto.CapacidadeMaxima > 0
            ? (ponto.QuantidadeAtual / ponto.CapacidadeMaxima) * 100m
            : 0m;

        if (percentual >= 100m)
        {
            if (!await _alertaRepository.ExisteAlertaPendenteAsync(ponto.Id, TipoAlerta.CapacidadeExcedida))
                await GerarAlertaAsync(ponto.Id, residuo.Id, TipoAlerta.CapacidadeExcedida,
                    $"Capacidade EXCEDIDA no ponto '{ponto.Nome}': {percentual:F1}% ({ponto.QuantidadeAtual}/{ponto.CapacidadeMaxima} kg)");
        }
        else if (percentual >= 90m)
        {
            if (!await _alertaRepository.ExisteAlertaPendenteAsync(ponto.Id, TipoAlerta.ColetaNecessaria))
                await GerarAlertaAsync(ponto.Id, residuo.Id, TipoAlerta.ColetaNecessaria,
                    $"Coleta necessária no ponto '{ponto.Nome}': {percentual:F1}% da capacidade atingida");
        }
        else
        {
            var limite = await _limiteRepository.GetByPontoEResiduoAsync(ponto.Id, residuo.Id);
            if (limite is null) return;

            // Verifica LimiteMaximo (quantidade absoluta) antes do percentual configurado
            if (ponto.QuantidadeAtual > limite.LimiteMaximo)
            {
                if (!await _alertaRepository.ExisteAlertaPendenteAsync(ponto.Id, TipoAlerta.CapacidadeAlerta))
                    await GerarAlertaAsync(ponto.Id, residuo.Id, TipoAlerta.CapacidadeAlerta,
                        $"Quantidade atual ({ponto.QuantidadeAtual} kg) excede o limite máximo configurado ({limite.LimiteMaximo} kg) no ponto '{ponto.Nome}'");
            }
            else if (percentual >= limite.PercentualAlerta)
            {
                if (!await _alertaRepository.ExisteAlertaPendenteAsync(ponto.Id, TipoAlerta.CapacidadeAlerta))
                    await GerarAlertaAsync(ponto.Id, residuo.Id, TipoAlerta.CapacidadeAlerta,
                        $"Alerta de capacidade no ponto '{ponto.Nome}': {percentual:F1}% atingido (limite configurado: {limite.PercentualAlerta}%)");
            }
        }
    }

    private async Task GerarAlertaAsync(int pontoColetaId, int residuoId, TipoAlerta tipo, string mensagem)
    {
        var alerta = new Alerta
        {
            PontoColetaId = pontoColetaId,
            ResiduoId = residuoId,
            Tipo = tipo,
            Mensagem = mensagem,
            Status = StatusAlerta.Pendente,
            DataAlerta = DateTime.Now
        };
        await _alertaRepository.AddAsync(alerta);
    }
}
