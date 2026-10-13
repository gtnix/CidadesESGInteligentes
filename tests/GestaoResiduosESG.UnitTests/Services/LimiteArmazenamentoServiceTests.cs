using AutoMapper;
using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Mappings;
using GestaoResiduosESG.Application.Services;
using GestaoResiduosESG.Application.ViewModels.Limite;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Services;

public class LimiteArmazenamentoServiceTests
{
    private readonly Mock<ILimiteArmazenamentoRepository> _limiteRepoMock;
    private readonly Mock<IPontoColetaRepository> _pontoRepoMock;
    private readonly Mock<IResiduoRepository> _residuoRepoMock;
    private readonly IMapper _mapper;

    public LimiteArmazenamentoServiceTests()
    {
        _limiteRepoMock = new Mock<ILimiteArmazenamentoRepository>();
        _pontoRepoMock  = new Mock<IPontoColetaRepository>();
        _residuoRepoMock = new Mock<IResiduoRepository>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private LimiteArmazenamentoService CreateService() =>
        new LimiteArmazenamentoService(
            _limiteRepoMock.Object,
            _pontoRepoMock.Object,
            _residuoRepoMock.Object,
            _mapper);

    [Fact]
    public async Task Create_LimiteMaxMenorQueMin_LancaBusinessException()
    {
        // Arrange
        var ponto   = new PontoColeta { Id = 1, Nome = "Ponto A" };
        var residuo = new Residuo    { Id = 1, Nome = "Papel" };

        _pontoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(residuo);

        var vm = new LimiteCreateViewModel
        {
            PontoColetaId = 1,
            ResiduoId = 1,
            LimiteMinimo = 200,
            LimiteMaximo = 100,  // inválido: max < min
            PercentualAlerta = 80
        };

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(vm));
    }

    [Fact]
    public async Task Create_LimiteJaExistente_LancaBusinessException()
    {
        // Arrange
        var ponto   = new PontoColeta { Id = 1, Nome = "Ponto A" };
        var residuo = new Residuo    { Id = 1, Nome = "Papel" };
        var existente = new LimiteArmazenamento { Id = 1, PontoColetaId = 1, ResiduoId = 1, LimiteMinimo = 10, LimiteMaximo = 500 };

        _pontoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ponto);
        _residuoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(residuo);
        _limiteRepoMock.Setup(r => r.GetByPontoEResiduoAsync(1, 1)).ReturnsAsync(existente);

        var vm = new LimiteCreateViewModel
        {
            PontoColetaId = 1,
            ResiduoId = 1,
            LimiteMinimo = 5,
            LimiteMaximo = 400,
            PercentualAlerta = 75
        };

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(vm));
    }

    [Fact]
    public async Task Update_LimiteMaxMenorQueMin_LancaBusinessException()
    {
        // Arrange
        var entity = new LimiteArmazenamento { Id = 1, PontoColetaId = 1, ResiduoId = 1, LimiteMinimo = 10, LimiteMaximo = 500, PercentualAlerta = 80 };
        _limiteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

        var vm = new LimiteUpdateViewModel
        {
            LimiteMinimo = 300,
            LimiteMaximo = 100,  // inválido: max < min
            PercentualAlerta = 80
        };

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(1, vm));
    }

    [Fact]
    public async Task Update_IdInexistente_RetornaNull()
    {
        // Arrange
        _limiteRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((LimiteArmazenamento?)null);

        var vm = new LimiteUpdateViewModel { LimiteMinimo = 10, LimiteMaximo = 200, PercentualAlerta = 80 };

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(99, vm);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_EntidadeExistente_RetornaTrue()
    {
        // Arrange
        var entity = new LimiteArmazenamento { Id = 3, PontoColetaId = 1, ResiduoId = 1, LimiteMinimo = 5, LimiteMaximo = 300 };
        _limiteRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(entity);
        _limiteRepoMock.Setup(r => r.DeleteAsync(entity)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(3);

        // Assert
        Assert.True(result);
        _limiteRepoMock.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Delete_EntidadeInexistente_RetornaFalse()
    {
        // Arrange
        _limiteRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((LimiteArmazenamento?)null);

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result);
        _limiteRepoMock.Verify(r => r.DeleteAsync(It.IsAny<LimiteArmazenamento>()), Times.Never);
    }
}
