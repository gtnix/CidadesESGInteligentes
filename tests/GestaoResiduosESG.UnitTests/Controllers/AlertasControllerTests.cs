using System.Net;
using System.Net.Http.Json;
using GestaoResiduosESG.Application.ViewModels.Alerta;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Controllers;

public class AlertasControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AlertasControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/alertas";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetWithStatusFilter_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/alertas?status=Pendente&page=1&pageSize=10";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsStatusCode404()
    {
        // Arrange
        var request = "/api/alertas/99999";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Resolver_NotFound_ReturnsStatusCode404()
    {
        // Arrange
        var vm = new AlertaResolverViewModel { Observacao = "Resolvido no teste" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/alertas/99999/resolver", vm);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
