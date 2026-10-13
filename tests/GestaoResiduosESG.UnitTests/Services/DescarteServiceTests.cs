using AutoMapper;
using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Mappings;
using GestaoResiduosESG.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using GestaoResiduosESG.Application.ViewModels.Descarte;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;
using Moq;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Services;

public class DescarteServiceTests
{
    private readonly Mock<IDescarteRepository> _descarteRepoMock;
    private readonly Mock<IPontoColetaRepository> _pontoRepoMock;
    private readonly Mock<IResiduoRepository> _residuoRepoMock;
    private readonly Mock<ILimiteArmazenamentoRepository> _limiteRepoMock;
    private readonly Mock<IAlertaRepository> _alertaRepoMock;
    private readonly Mock<IPontoResiduoRepository> _pontoResiduoRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public DescarteServiceTests()
    {
        _descarteRepoMock = new Mock<IDescarteRepository>();
        _pontoRepoMock = new Mock<IPontoColetaRepository>();
        _residuoRepoMock = new Mock<IResiduoRepository>();
        _limiteRepoMock = new Mock<ILimiteArmazenamentoRepository>();
        _alertaRepoMock = new Mock<IAlertaRepository>();
        _pontoResiduoRepoMock = new Mock<IPontoResiduoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        var sp = services.BuildServiceProvider();
        _mapper = sp.GetRequiredService<IMapper>();
    }

    private DescarteService CreateService() =>
        new DescarteService(
            _descarteRepoMock.Object,
            _pontoRepoMock.Object,
            _residuoRepoMock.Object,
            _limiteRepoMock.Object,
            _alertaRepoMock.Object,
            _pontoResiduoRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapper);

    [Fact]
    public async Task CreateDescarte_ResiduoNaoAceitoNoPonto_LancaBusinessException()
    {
        // Arrange
        var ponto = new PontoColeta { Id = 1, Nome = "Ponto A", CapacidadeMaxima = 100, QuantidadeAtual = 0 };
        var residuo = new Residuo { Id = 1, Nome = "Papel" };

        _pontoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(residuo);
        _pontoResiduoRepoMock.Setup(r => r.ExisteAsync(1, 1)).ReturnsAsync(false); // Não aceito

        var vm = new DescarteCreateViewModel
        {
            PontoColetaId = 1,
            ResiduoId = 1,
            Quantidade = 10,
            DataDescarte = DateTime.Now.AddMinutes(-1),
            Responsavel = "Operador"
        };

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(vm));
    }

    [Fact]
    public async Task CreateDescarte_AcimaDe80Porcento_GeraAlertaCapacidade()
    {
        // Arrange — ponto em 85% de capacidade após o descarte
        var ponto = new PontoColeta { Id = 1, Nome = "Ponto B", CapacidadeMaxima = 100, QuantidadeAtual = 75 };
        var residuo = new Residuo { Id = 1, Nome = "Plástico", Categoria = CategoriaResiduo.Reciclavel };
        var limite = new LimiteArmazenamento
        {
            Id = 1, PontoColetaId = 1, ResiduoId = 1,
            PercentualAlerta = 80, LimiteMinimo = 0, LimiteMaximo = 100
        };
        var descarteCriado = new Descarte
        {
            Id = 1, PontoColetaId = 1, ResiduoId = 1,
            Quantidade = 10, DataDescarte = DateTime.Now, Responsavel = "Operador",
            PontoColeta = ponto, Residuo = residuo
        };

        _pontoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(residuo);
        _pontoResiduoRepoMock.Setup(r => r.ExisteAsync(1, 1)).ReturnsAsync(true);
        _descarteRepoMock.Setup(r => r.AddAsync(It.IsAny<Descarte>())).ReturnsAsync(descarteCriado);
        _descarteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(descarteCriado);
        _limiteRepoMock.Setup(r => r.GetByPontoEResiduoAsync(1, 1)).ReturnsAsync(limite);
        _alertaRepoMock.Setup(r => r.ExisteAlertaPendenteAsync(1, TipoAlerta.CapacidadeAlerta)).ReturnsAsync(false);
        _alertaRepoMock.Setup(r => r.AddAsync(It.IsAny<Alerta>())).ReturnsAsync(new Alerta());

        var vm = new DescarteCreateViewModel
        {
            PontoColetaId = 1, ResiduoId = 1,
            Quantidade = 10, DataDescarte = DateTime.Now.AddMinutes(-1),
            Responsavel = "Operador"
        };

        var service = CreateService();

        // Act
        await service.CreateAsync(vm);

        // Assert — alerta de capacidade deve ter sido gerado
        _alertaRepoMock.Verify(
            r => r.AddAsync(It.Is<Alerta>(a => a.Tipo == TipoAlerta.CapacidadeAlerta)),
            Times.Once);
    }

    [Fact]
    public async Task CreateDescarte_AcimaDe90Porcento_GeraAlertaColetaNecessaria()
    {
        // Arrange — ponto em 95% após o descarte
        var ponto = new PontoColeta { Id = 2, Nome = "Ponto C", CapacidadeMaxima = 100, QuantidadeAtual = 90 };
        var residuo = new Residuo { Id = 2, Nome = "Vidro", Categoria = CategoriaResiduo.Reciclavel };
        var descarteCriado = new Descarte
        {
            Id = 2, PontoColetaId = 2, ResiduoId = 2,
            Quantidade = 5, DataDescarte = DateTime.Now, Responsavel = "Operador",
            PontoColeta = ponto, Residuo = residuo
        };

        _pontoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(residuo);
        _pontoResiduoRepoMock.Setup(r => r.ExisteAsync(2, 2)).ReturnsAsync(true);
        _descarteRepoMock.Setup(r => r.AddAsync(It.IsAny<Descarte>())).ReturnsAsync(descarteCriado);
        _descarteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(descarteCriado);
        _alertaRepoMock.Setup(r => r.ExisteAlertaPendenteAsync(2, TipoAlerta.ColetaNecessaria)).ReturnsAsync(false);
        _alertaRepoMock.Setup(r => r.AddAsync(It.IsAny<Alerta>())).ReturnsAsync(new Alerta());

        var vm = new DescarteCreateViewModel
        {
            PontoColetaId = 2, ResiduoId = 2,
            Quantidade = 5, DataDescarte = DateTime.Now.AddMinutes(-1),
            Responsavel = "Operador"
        };

        var service = CreateService();

        // Act
        await service.CreateAsync(vm);

        // Assert — alerta de coleta necessária deve ter sido gerado
        _alertaRepoMock.Verify(
            r => r.AddAsync(It.Is<Alerta>(a => a.Tipo == TipoAlerta.ColetaNecessaria)),
            Times.Once);
    }
}
