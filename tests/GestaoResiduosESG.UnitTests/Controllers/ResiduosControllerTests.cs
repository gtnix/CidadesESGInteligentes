using System.Net;
using System.Net.Http.Json;
using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Domain.Enums;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Controllers;

public class ResiduosControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResiduosControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/residuos";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsStatusCode404()
    {
        // Arrange
        var request = "/api/residuos/99999";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidData_EmptyName_ReturnsStatusCode400()
    {
        // Arrange
        var vm = new { Nome = "", Categoria = "Organico", UnidadeMedida = "kg" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/residuos", vm);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidData_ReturnsStatusCode201()
    {
        // Arrange
        var vm = new ResiduoCreateViewModel
        {
            Nome = "Papel Teste",
            Categoria = CategoriaResiduo.Reciclavel,
            UnidadeMedida = "kg"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/residuos", vm);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsStatusCode200()
    {
        // Arrange
        var request = "/api/residuos?page=1&pageSize=5";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Update_ExistingResiduo_ReturnsStatusCode204()
    {
        // Arrange — cria um resíduo para depois atualizar
        var create = new ResiduoCreateViewModel
        {
            Nome = "Vidro Para Atualizar",
            Categoria = CategoriaResiduo.Reciclavel,
            UnidadeMedida = "kg"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/residuos", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ResiduoResponseViewModel>();
        Assert.NotNull(created);

        var update = new ResiduoUpdateViewModel
        {
            Nome = "Vidro Atualizado",
            Categoria = CategoriaResiduo.NaoReciclavel,
            UnidadeMedida = "un"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/residuos/{created.Id}", update);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExistentResiduo_ReturnsStatusCode404()
    {
        // Arrange
        var update = new ResiduoUpdateViewModel
        {
            Nome = "Não Existe",
            Categoria = CategoriaResiduo.Organico,
            UnidadeMedida = "kg"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/residuos/99999", update);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingResiduo_ReturnsStatusCode204()
    {
        // Arrange — cria um resíduo para depois excluir
        var create = new ResiduoCreateViewModel
        {
            Nome = "Resíduo Para Excluir",
            Categoria = CategoriaResiduo.Perigoso,
            UnidadeMedida = "kg"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/residuos", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ResiduoResponseViewModel>();
        Assert.NotNull(created);

        // Act
        var response = await _client.DeleteAsync($"/api/residuos/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentResiduo_ReturnsStatusCode404()
    {
        // Act
        var response = await _client.DeleteAsync("/api/residuos/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_NomeInvalido_ReturnsStatusCode400(string nome)
    {
        // Arrange
        var vm = new { Nome = nome, Categoria = "Organico", UnidadeMedida = "kg" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/residuos", vm);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    public async Task GetAll_DiferentesPageSizes_ReturnsStatusCode200(int pageSize)
    {
        // Arrange
        var request = $"/api/residuos?page=1&pageSize={pageSize}";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
