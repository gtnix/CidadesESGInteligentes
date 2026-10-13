using AutoMapper;
using GestaoResiduosESG.Application.Mappings;
using GestaoResiduosESG.Application.Services;
using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using GestaoResiduosESG.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Services;

public class ResiduoServiceTests
{
    private readonly Mock<IResiduoRepository> _repositoryMock;
    private readonly IMapper _mapper;

    public ResiduoServiceTests()
    {
        _repositoryMock = new Mock<IResiduoRepository>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        var sp = services.BuildServiceProvider();
        _mapper = sp.GetRequiredService<IMapper>();
    }

    private ResiduoService CreateService() =>
        new ResiduoService(_repositoryMock.Object, _mapper);

    [Fact]
    public async Task GetById_IdExistente_RetornaViewModel()
    {
        // Arrange
        var residuo = new Residuo
        {
            Id = 1,
            Nome = "Papel",
            Categoria = CategoriaResiduo.Reciclavel,
            UnidadeMedida = "kg"
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(residuo);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Papel", result.Nome);
    }

    [Fact]
    public async Task GetById_IdInexistente_RetornaNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Residuo?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Create_ViewModelValido_RetornaResponseViewModel()
    {
        // Arrange
        var vm = new ResiduoCreateViewModel
        {
            Nome = "Vidro",
            Categoria = CategoriaResiduo.Reciclavel,
            UnidadeMedida = "un"
        };

        var entidadeSalva = new Residuo
        {
            Id = 10,
            Nome = vm.Nome,
            Categoria = vm.Categoria,
            UnidadeMedida = vm.UnidadeMedida
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Residuo>()))
            .ReturnsAsync(entidadeSalva);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(vm);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Vidro", result.Nome);
    }

    [Fact]
    public async Task Delete_EntidadeExistente_RetornaTrue()
    {
        // Arrange
        var residuo = new Residuo { Id = 5, Nome = "Plástico", Categoria = CategoriaResiduo.Reciclavel, UnidadeMedida = "kg" };
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(residuo);
        _repositoryMock.Setup(r => r.DeleteAsync(residuo)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(5);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(residuo), Times.Once);
    }

    [Fact]
    public async Task Delete_EntidadeInexistente_RetornaFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Residuo?)null);

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Residuo>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_RetornaPaginado()
    {
        // Arrange
        var residuos = new List<Residuo>
        {
            new() { Id = 1, Nome = "Papel", Categoria = CategoriaResiduo.Reciclavel, UnidadeMedida = "kg" },
            new() { Id = 2, Nome = "Vidro", Categoria = CategoriaResiduo.Reciclavel, UnidadeMedida = "un" }
        };
        _repositoryMock.Setup(r => r.GetAllPagedAsync(1, 10)).ReturnsAsync((residuos, 2));

        var service = CreateService();

        // Act
        var result = await service.GetAllAsync(1, 10);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Data.Count());
    }
}
