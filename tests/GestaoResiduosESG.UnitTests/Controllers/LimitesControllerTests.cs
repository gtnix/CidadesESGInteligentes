using System.Net;
using System.Net.Http.Json;
using GestaoResiduosESG.Application.ViewModels.Limite;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Enums;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Controllers;

public class LimitesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LimitesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/limites";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsStatusCode404()
    {
        // Arrange
        var request = "/api/limites/99999";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidData_LimiteMaxMenorQueMin_ReturnsStatusCode400()
    {
        // Arrange — limiteMaximo < limiteMinimo deve ser rejeitado pela validação de negócio
        var vm = new LimiteCreateViewModel
        {
            PontoColetaId = 1,
            ResiduoId = 1,
            LimiteMinimo = 100,
            LimiteMaximo = 50,   // inválido: max < min
            PercentualAlerta = 80
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/limites", vm);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidData_PontoIdZero_ReturnsStatusCode400()
    {
        // Arrange — PontoColetaId = 0 viola [Range(1, int.MaxValue)]
        var vm = new { PontoColetaId = 0, ResiduoId = 1, LimiteMinimo = 10, LimiteMaximo = 100, PercentualAlerta = 80 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/limites", vm);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsStatusCode200()
    {
        // Arrange
        var request = "/api/limites?page=1&pageSize=5";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetAll_ResponseContainsPaginationFields()
    {
        // Act
        var response = await _client.GetAsync("/api/limites?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedResponseViewModel<LimiteResponseViewModel>>();
        Assert.NotNull(body);
        Assert.Equal(1, body.Page);
        Assert.Equal(10, body.PageSize);
    }

    private async Task<(int pontoId, int residuoId)> CriarPontoEResiduoAsync()
    {
        var ponto = new PontoColetaCreateViewModel { Nome = "Ponto Limite Teste", Localizacao = "Área B", CapacidadeMaxima = 1000 };
        var pontoResp = await _client.PostAsJsonAsync("/api/pontos-coleta", ponto);
        var pontoCreated = await pontoResp.Content.ReadFromJsonAsync<PontoColetaResponseViewModel>();

        var residuo = new ResiduoCreateViewModel { Nome = "Resíduo Limite Teste", Categoria = CategoriaResiduo.Reciclavel, UnidadeMedida = "kg" };
        var residuoResp = await _client.PostAsJsonAsync("/api/residuos", residuo);
        var residuoCreated = await residuoResp.Content.ReadFromJsonAsync<ResiduoResponseViewModel>();

        return (pontoCreated!.Id, residuoCreated!.Id);
    }

    [Fact]
    public async Task Create_ValidData_ReturnsStatusCode201()
    {
        // Arrange — cria ponto e resíduo para o limite
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();

        var vm = new LimiteCreateViewModel
        {
            PontoColetaId = pontoId,
            ResiduoId = residuoId,
            LimiteMinimo = 10,
            LimiteMaximo = 500,
            PercentualAlerta = 80
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/limites", vm);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingLimite_ReturnsStatusCode204()
    {
        // Arrange — cria ponto, resíduo e limite
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();

        var create = new LimiteCreateViewModel
        {
            PontoColetaId = pontoId,
            ResiduoId = residuoId,
            LimiteMinimo = 5,
            LimiteMaximo = 200,
            PercentualAlerta = 75
        };
        var createResp = await _client.PostAsJsonAsync("/api/limites", create);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.Content.ReadFromJsonAsync<LimiteResponseViewModel>();
        Assert.NotNull(created);

        var update = new LimiteUpdateViewModel
        {
            LimiteMinimo = 10,
            LimiteMaximo = 400,
            PercentualAlerta = 85
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/limites/{created.Id}", update);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExistentLimite_ReturnsStatusCode404()
    {
        // Arrange
        var update = new LimiteUpdateViewModel { LimiteMinimo = 10, LimiteMaximo = 200, PercentualAlerta = 80 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/limites/99999", update);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingLimite_ReturnsStatusCode204()
    {
        // Arrange — cria ponto, resíduo e limite
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();

        var create = new LimiteCreateViewModel
        {
            PontoColetaId = pontoId,
            ResiduoId = residuoId,
            LimiteMinimo = 0,
            LimiteMaximo = 300,
            PercentualAlerta = 90
        };
        var createResp = await _client.PostAsJsonAsync("/api/limites", create);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.Content.ReadFromJsonAsync<LimiteResponseViewModel>();
        Assert.NotNull(created);

        // Act
        var response = await _client.DeleteAsync($"/api/limites/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentLimite_ReturnsStatusCode404()
    {
        // Act
        var response = await _client.DeleteAsync("/api/limites/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0, 100, 80)]
    [InlineData(-1, 100, 80)]
    public async Task Create_PontoColetaIdInvalido_ReturnsStatusCode400(int pontoColetaId, int limiteMaximo, int percentualAlerta)
    {
        // Arrange
        var vm = new { PontoColetaId = pontoColetaId, ResiduoId = 1, LimiteMinimo = 0, LimiteMaximo = limiteMaximo, PercentualAlerta = percentualAlerta };

        // Act
        var response = await _client.PostAsJsonAsync("/api/limites", vm);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 50)]
    public async Task GetAll_PaginacaoVariada_ReturnsStatusCode200(int page, int pageSize)
    {
        // Arrange
        var request = $"/api/limites?page={page}&pageSize={pageSize}";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
