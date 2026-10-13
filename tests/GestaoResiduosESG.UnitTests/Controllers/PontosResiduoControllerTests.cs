using System.Net;
using System.Net.Http.Json;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using GestaoResiduosESG.Application.ViewModels.PontoResiduo;
using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Enums;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Controllers;

public class PontosResiduoControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PontosResiduoControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Auxiliar — cria ponto de coleta e resíduo para uso nos testes
    private async Task<(int pontoId, int residuoId)> CriarPontoEResiduoAsync()
    {
        var ponto = new PontoColetaCreateViewModel
        {
            Nome = "Ponto Vínculo Teste",
            Localizacao = "Área Teste",
            CapacidadeMaxima = 500
        };
        var pontoResp = await _client.PostAsJsonAsync("/api/pontos-coleta", ponto);
        Assert.Equal(HttpStatusCode.Created, pontoResp.StatusCode);
        var pontoCreated = await pontoResp.Content.ReadFromJsonAsync<PontoColetaResponseViewModel>();
        Assert.NotNull(pontoCreated);

        var residuo = new ResiduoCreateViewModel
        {
            Nome = "Resíduo Vínculo Teste",
            Categoria = CategoriaResiduo.Reciclavel,
            UnidadeMedida = "kg"
        };
        var residuoResp = await _client.PostAsJsonAsync("/api/residuos", residuo);
        Assert.Equal(HttpStatusCode.Created, residuoResp.StatusCode);
        var residuoCreated = await residuoResp.Content.ReadFromJsonAsync<ResiduoResponseViewModel>();
        Assert.NotNull(residuoCreated);

        return (pontoCreated.Id, residuoCreated.Id);
    }

    [Fact]
    public async Task GetByPonto_ReturnsHttpStatusCode200()
    {
        // Arrange
        var (pontoId, _) = await CriarPontoEResiduoAsync();
        var request = $"/api/pontos-coleta/{pontoId}/residuos";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetByPonto_RetornaListaPaginada()
    {
        // Arrange
        var (pontoId, _) = await CriarPontoEResiduoAsync();

        // Act
        var response = await _client.GetAsync($"/api/pontos-coleta/{pontoId}/residuos?page=1&pageSize=20");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedResponseViewModel<PontoResiduoResponseViewModel>>();
        Assert.NotNull(body);
        Assert.Equal(1, body.Page);
        Assert.Equal(20, body.PageSize);
    }

    [Fact]
    public async Task Vincular_ResiduoValido_ReturnsStatusCode204()
    {
        // Arrange
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();

        // Act
        var response = await _client.PostAsync($"/api/pontos-coleta/{pontoId}/residuos/{residuoId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Vincular_ResidueJaVinculado_ReturnsStatusCode400()
    {
        // Arrange — vincula uma primeira vez
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();
        var primeiroVinculo = await _client.PostAsync($"/api/pontos-coleta/{pontoId}/residuos/{residuoId}", null);
        Assert.Equal(HttpStatusCode.NoContent, primeiroVinculo.StatusCode);

        // Act — tenta vincular novamente (deve retornar 400 por BusinessException)
        var response = await _client.PostAsync($"/api/pontos-coleta/{pontoId}/residuos/{residuoId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Vincular_PontoInexistente_ReturnsStatusCode404()
    {
        // Arrange
        var (_, residuoId) = await CriarPontoEResiduoAsync();

        // Act
        var response = await _client.PostAsync($"/api/pontos-coleta/99999/residuos/{residuoId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Vincular_ResiduoInexistente_ReturnsStatusCode404()
    {
        // Arrange
        var (pontoId, _) = await CriarPontoEResiduoAsync();

        // Act
        var response = await _client.PostAsync($"/api/pontos-coleta/{pontoId}/residuos/99999", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Desvincular_VinculoExistente_ReturnsStatusCode204()
    {
        // Arrange — vincula primeiro para depois desvincular
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();
        var vincular = await _client.PostAsync($"/api/pontos-coleta/{pontoId}/residuos/{residuoId}", null);
        Assert.Equal(HttpStatusCode.NoContent, vincular.StatusCode);

        // Act
        var response = await _client.DeleteAsync($"/api/pontos-coleta/{pontoId}/residuos/{residuoId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Desvincular_VinculoInexistente_ReturnsStatusCode404()
    {
        // Arrange
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();

        // Act — tenta desvincular sem ter vinculado antes
        var response = await _client.DeleteAsync($"/api/pontos-coleta/{pontoId}/residuos/{residuoId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByPonto_AposVincular_RetornaResiduoNaLista()
    {
        // Arrange — cria ponto e resíduo, vincula
        var (pontoId, residuoId) = await CriarPontoEResiduoAsync();
        var vincular = await _client.PostAsync($"/api/pontos-coleta/{pontoId}/residuos/{residuoId}", null);
        Assert.Equal(HttpStatusCode.NoContent, vincular.StatusCode);

        // Act
        var response = await _client.GetAsync($"/api/pontos-coleta/{pontoId}/residuos");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedResponseViewModel<PontoResiduoResponseViewModel>>();
        Assert.NotNull(body);
        Assert.Equal(1, body.TotalCount);
        Assert.Contains(body.Data, pr => pr.ResiduoId == residuoId);
    }
}
