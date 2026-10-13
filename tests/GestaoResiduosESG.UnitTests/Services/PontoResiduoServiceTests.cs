using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Services;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;
using Moq;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Services;

public class PontoResiduoServiceTests
{
    private readonly Mock<IPontoResiduoRepository> _pontoResiduoRepoMock;
    private readonly Mock<IPontoColetaRepository> _pontoColetaRepoMock;
    private readonly Mock<IResiduoRepository> _residuoRepoMock;

    public PontoResiduoServiceTests()
    {
        _pontoResiduoRepoMock = new Mock<IPontoResiduoRepository>();
        _pontoColetaRepoMock  = new Mock<IPontoColetaRepository>();
        _residuoRepoMock      = new Mock<IResiduoRepository>();
    }

    private PontoResiduoService CreateService() =>
        new PontoResiduoService(
            _pontoResiduoRepoMock.Object,
            _pontoColetaRepoMock.Object,
            _residuoRepoMock.Object);

    [Fact]
    public async Task Vincular_PontoEResidioExistentes_VinculaChamandoAddAsync()
    {
        // Arrange
        var ponto   = new PontoColeta { Id = 1, Nome = "Ponto A" };
        var residuo = new Residuo    { Id = 2, Nome = "Papel" };

        _pontoColetaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(residuo);
        _pontoResiduoRepoMock.Setup(r => r.ExisteAsync(1, 2)).ReturnsAsync(false);
        _pontoResiduoRepoMock.Setup(r => r.AddAsync(It.IsAny<PontoResiduo>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.VincularAsync(1, 2);

        // Assert
        _pontoResiduoRepoMock.Verify(r => r.AddAsync(It.Is<PontoResiduo>(pr => pr.PontoColetaId == 1 && pr.ResiduoId == 2)), Times.Once);
    }

    [Fact]
    public async Task Vincular_PontoInexistente_LancaNotFoundException()
    {
        // Arrange
        _pontoColetaRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PontoColeta?)null);

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.VincularAsync(99, 1));
    }

    [Fact]
    public async Task Vincular_ResiduoInexistente_LancaNotFoundException()
    {
        // Arrange
        var ponto = new PontoColeta { Id = 1, Nome = "Ponto A" };
        _pontoColetaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Residuo?)null);

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.VincularAsync(1, 99));
    }

    [Fact]
    public async Task Vincular_JaVinculado_LancaBusinessException()
    {
        // Arrange
        var ponto   = new PontoColeta { Id = 1, Nome = "Ponto A" };
        var residuo = new Residuo    { Id = 2, Nome = "Papel" };

        _pontoColetaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(residuo);
        _pontoResiduoRepoMock.Setup(r => r.ExisteAsync(1, 2)).ReturnsAsync(true); // já vinculado

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => service.VincularAsync(1, 2));
    }

    [Fact]
    public async Task Desvincular_VinculoExistente_ChamaDeleteAsync()
    {
        // Arrange
        _pontoResiduoRepoMock.Setup(r => r.ExisteAsync(1, 2)).ReturnsAsync(true);
        _pontoResiduoRepoMock.Setup(r => r.DeleteAsync(1, 2)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.DesvincularAsync(1, 2);

        // Assert
        _pontoResiduoRepoMock.Verify(r => r.DeleteAsync(1, 2), Times.Once);
    }

    [Fact]
    public async Task Desvincular_VinculoInexistente_LancaNotFoundException()
    {
        // Arrange
        _pontoResiduoRepoMock.Setup(r => r.ExisteAsync(1, 99)).ReturnsAsync(false);

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.DesvincularAsync(1, 99));
    }
}
