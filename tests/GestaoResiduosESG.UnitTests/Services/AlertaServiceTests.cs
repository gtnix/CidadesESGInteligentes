using AutoMapper;
using GestaoResiduosESG.Application.Exceptions;
using GestaoResiduosESG.Application.Mappings;
using GestaoResiduosESG.Application.Services;
using GestaoResiduosESG.Application.ViewModels.Alerta;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Services;

public class AlertaServiceTests
{
    private readonly Mock<IAlertaRepository> _repositoryMock;
    private readonly IMapper _mapper;

    public AlertaServiceTests()
    {
        _repositoryMock = new Mock<IAlertaRepository>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        var sp = services.BuildServiceProvider();
        _mapper = sp.GetRequiredService<IMapper>();
    }

    private AlertaService CreateService() =>
        new AlertaService(_repositoryMock.Object, _mapper);

    [Fact]
    public async Task GetById_AlertaExistente_RetornaViewModel()
    {
        // Arrange
        var alerta = new Alerta
        {
            Id = 1,
            PontoColetaId = 1,
            ResiduoId = 1,
            Tipo = TipoAlerta.CapacidadeAlerta,
            Mensagem = "Capacidade em alerta",
            Status = StatusAlerta.Pendente,
            DataAlerta = DateTime.Now,
            PontoColeta = new PontoColeta { Nome = "Ponto A" },
            Residuo = new Residuo { Nome = "Papel" }
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alerta);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetById_AlertaInexistente_RetornaNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Alerta?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Resolver_AlertaPendente_MudaStatusParaResolvido()
    {
        // Arrange
        var alerta = new Alerta
        {
            Id = 2,
            PontoColetaId = 1,
            ResiduoId = 1,
            Tipo = TipoAlerta.ColetaNecessaria,
            Mensagem = "Coleta necessária",
            Status = StatusAlerta.Pendente,
            DataAlerta = DateTime.Now,
            PontoColeta = new PontoColeta { Nome = "Ponto B" },
            Residuo = new Residuo { Nome = "Vidro" }
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(alerta);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Alerta>())).Returns(Task.CompletedTask);

        var service = CreateService();
        var vm = new AlertaResolverViewModel { Observacao = "Coleta realizada" };

        // Act
        var result = await service.ResolverAsync(2, vm);

        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Alerta>(a => a.Status == StatusAlerta.Resolvido)), Times.Once);
    }

    [Fact]
    public async Task Resolver_AlertaJaResolvido_LancaBusinessException()
    {
        // Arrange
        var alerta = new Alerta
        {
            Id = 3,
            PontoColetaId = 1,
            ResiduoId = 1,
            Tipo = TipoAlerta.CapacidadeExcedida,
            Mensagem = "Alerta resolvido",
            Status = StatusAlerta.Resolvido,
            DataAlerta = DateTime.Now,
            PontoColeta = new PontoColeta { Nome = "Ponto C" },
            Residuo = new Residuo { Nome = "Metal" }
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(alerta);

        var service = CreateService();
        var vm = new AlertaResolverViewModel { Observacao = "Tentativa" };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => service.ResolverAsync(3, vm));
    }

    [Fact]
    public async Task Resolver_AlertaInexistente_RetornaNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Alerta?)null);

        var service = CreateService();
        var vm = new AlertaResolverViewModel { Observacao = "Teste" };

        // Act
        var result = await service.ResolverAsync(999, vm);

        // Assert
        Assert.Null(result);
    }
}
