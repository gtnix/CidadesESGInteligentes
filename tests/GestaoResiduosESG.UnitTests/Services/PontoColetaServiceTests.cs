using AutoMapper;
using GestaoResiduosESG.Application.Mappings;
using GestaoResiduosESG.Application.Services;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Services;

public class PontoColetaServiceTests
{
    private readonly Mock<IPontoColetaRepository> _pontoRepoMock;
    private readonly Mock<IAlertaRepository> _alertaRepoMock;
    private readonly Mock<ILimiteArmazenamentoRepository> _limiteRepoMock;
    private readonly Mock<IPontoResiduoRepository> _pontoResiduoRepoMock;
    private readonly IMapper _mapper;

    public PontoColetaServiceTests()
    {
        _pontoRepoMock       = new Mock<IPontoColetaRepository>();
        _alertaRepoMock      = new Mock<IAlertaRepository>();
        _limiteRepoMock      = new Mock<ILimiteArmazenamentoRepository>();
        _pontoResiduoRepoMock = new Mock<IPontoResiduoRepository>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private PontoColetaService CreateService() =>
        new PontoColetaService(
            _pontoRepoMock.Object,
            _alertaRepoMock.Object,
            _limiteRepoMock.Object,
            _pontoResiduoRepoMock.Object,
            _mapper);

    [Fact]
    public async Task GetById_IdExistente_RetornaViewModel()
    {
        // Arrange
        var ponto = new PontoColeta { Id = 1, Nome = "Ponto A", Localizacao = "Bloco A", CapacidadeMaxima = 500, QuantidadeAtual = 100 };
        _pontoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ponto A", result.Nome);
    }

    [Fact]
    public async Task GetById_IdInexistente_RetornaNull()
    {
        // Arrange
        _pontoRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PontoColeta?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatus_CalculaPercentualCorretamente()
    {
        // Arrange — ponto com 250 de 500 (50%)
        var ponto = new PontoColeta { Id = 1, Nome = "Ponto B", Localizacao = "Bloco B", CapacidadeMaxima = 500, QuantidadeAtual = 250 };
        _pontoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _alertaRepoMock.Setup(r => r.GetByPontoColetaAsync(1)).ReturnsAsync(Enumerable.Empty<Alerta>());

        var service = CreateService();

        // Act
        var result = await service.GetStatusAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50m, result.PercentualOcupacao);
        Assert.Equal("Normal", result.StatusOcupacao);
    }

    [Fact]
    public async Task GetStatus_AcimaDe80Porcento_RetornaStatusAlerta()
    {
        // Arrange — ponto em 85% de capacidade
        var ponto = new PontoColeta { Id = 2, Nome = "Ponto C", Localizacao = "Bloco C", CapacidadeMaxima = 100, QuantidadeAtual = 85 };
        _pontoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(ponto);
        _alertaRepoMock.Setup(r => r.GetByPontoColetaAsync(2)).ReturnsAsync(Enumerable.Empty<Alerta>());

        var service = CreateService();

        // Act
        var result = await service.GetStatusAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(85m, result.PercentualOcupacao);
        Assert.Equal("Alerta", result.StatusOcupacao);
    }

    [Fact]
    public async Task GetStatus_Cheio_RetornaStatusCheio()
    {
        // Arrange — ponto em 100%
        var ponto = new PontoColeta { Id = 3, Nome = "Ponto D", Localizacao = "Bloco D", CapacidadeMaxima = 100, QuantidadeAtual = 100 };
        _pontoRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(ponto);
        _alertaRepoMock.Setup(r => r.GetByPontoColetaAsync(3)).ReturnsAsync(Enumerable.Empty<Alerta>());

        var service = CreateService();

        // Act
        var result = await service.GetStatusAsync(3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cheio", result.StatusOcupacao);
    }

    [Fact]
    public async Task RegistrarColeta_PontoExistente_ZeraQuantidade()
    {
        // Arrange
        var ponto = new PontoColeta { Id = 1, Nome = "Ponto E", Localizacao = "Bloco E", CapacidadeMaxima = 200, QuantidadeAtual = 150 };
        _pontoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _pontoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<PontoColeta>())).Returns(Task.CompletedTask);
        _alertaRepoMock.Setup(r => r.GetByPontoColetaAsync(1)).ReturnsAsync(Enumerable.Empty<Alerta>());
        _pontoResiduoRepoMock.Setup(r => r.GetByPontoColetaAsync(1)).ReturnsAsync(Enumerable.Empty<PontoResiduo>());

        var service = CreateService();

        // Act
        var result = await service.RegistrarColetaAsync(1);

        // Assert
        Assert.True(result);
        _pontoRepoMock.Verify(r => r.UpdateAsync(It.Is<PontoColeta>(p => p.QuantidadeAtual == 0)), Times.Once);
    }

    [Fact]
    public async Task RegistrarColeta_PontoInexistente_RetornaFalse()
    {
        // Arrange
        _pontoRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PontoColeta?)null);

        var service = CreateService();

        // Act
        var result = await service.RegistrarColetaAsync(99);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Delete_PontoExistente_RetornaTrue()
    {
        // Arrange
        var ponto = new PontoColeta { Id = 5, Nome = "Ponto F", Localizacao = "Bloco F", CapacidadeMaxima = 100, QuantidadeAtual = 0 };
        _pontoRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(ponto);
        _pontoRepoMock.Setup(r => r.DeleteAsync(ponto)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(5);

        // Assert
        Assert.True(result);
        _pontoRepoMock.Verify(r => r.DeleteAsync(ponto), Times.Once);
    }
}
