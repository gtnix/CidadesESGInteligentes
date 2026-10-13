using System.Net;
using System.Net.Http.Json;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Controllers;

public class PontosColetaControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PontosColetaControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/pontos-coleta";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsStatusCode404()
    {
        // Arrange
        var request = "/api/pontos-coleta/99999";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStatus_NotFound_ReturnsStatusCode404()
    {
        // Arrange
        var request = "/api/pontos-coleta/99999/status";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidData_NegativeCapacity_ReturnsStatusCode400()
    {
        // Arrange
        var vm = new { Nome = "Ponto A", Localizacao = "Sala 1", CapacidadeMaxima = -10 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pontos-coleta", vm);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidData_ReturnsStatusCode201()
    {
        // Arrange
        var vm = new PontoColetaCreateViewModel
        {
            Nome = "Ponto Central Teste",
            Localizacao = "Refeitório",
            CapacidadeMaxima = 500
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pontos-coleta", vm);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetStatus_ExistingPoint_ReturnsStatusCode200()
    {
        // Arrange: criar ponto
        var create = new PontoColetaCreateViewModel
        {
            Nome = "Ponto Status Teste",
            Localizacao = "Lab",
            CapacidadeMaxima = 200
        };
        var createResponse = await _client.PostAsJsonAsync("/api/pontos-coleta", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);

        var statusUrl = location.ToString().TrimEnd('/') + "/status";

        // Act
        var response = await _client.GetAsync(statusUrl);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Update_ExistingPoint_ReturnsStatusCode204()
    {
        // Arrange — cria um ponto para depois atualizar
        var create = new PontoColetaCreateViewModel
        {
            Nome = "Ponto Para Atualizar",
            Localizacao = "Depósito",
            CapacidadeMaxima = 300
        };
        var createResponse = await _client.PostAsJsonAsync("/api/pontos-coleta", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PontoColetaResponseViewModel>();
        Assert.NotNull(created);

        var update = new PontoColetaUpdateViewModel
        {
            Nome = "Ponto Atualizado",
            Localizacao = "Depósito Central",
            CapacidadeMaxima = 600
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/pontos-coleta/{created.Id}", update);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExistentPoint_ReturnsStatusCode404()
    {
        // Arrange
        var update = new PontoColetaUpdateViewModel
        {
            Nome = "Não Existe",
            Localizacao = "Nenhum lugar",
            CapacidadeMaxima = 100
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/pontos-coleta/99999", update);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingPoint_ReturnsStatusCode204()
    {
        // Arrange — cria um ponto para depois excluir
        var create = new PontoColetaCreateViewModel
        {
            Nome = "Ponto Para Excluir",
            Localizacao = "Garagem",
            CapacidadeMaxima = 100
        };
        var createResponse = await _client.PostAsJsonAsync("/api/pontos-coleta", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PontoColetaResponseViewModel>();
        Assert.NotNull(created);

        // Act
        var response = await _client.DeleteAsync($"/api/pontos-coleta/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentPoint_ReturnsStatusCode404()
    {
        // Act
        var response = await _client.DeleteAsync("/api/pontos-coleta/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
