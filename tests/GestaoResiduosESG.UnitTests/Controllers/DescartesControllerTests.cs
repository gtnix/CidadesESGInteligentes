using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GestaoResiduosESG.UnitTests.Controllers;

public class DescartesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DescartesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/descartes";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsStatusCode404()
    {
        // Arrange
        var request = "/api/descartes/99999";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidData_ZeroQuantity_ReturnsStatusCode400()
    {
        // Arrange
        var vm = new
        {
            ResiduoId = 1,
            PontoColetaId = 1,
            DataDescarte = DateTime.Now,
            Quantidade = 0,
            Responsavel = "Teste"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/descartes", vm);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_PontoOuResiduoInexistente_ReturnsStatusCode404()
    {
        // Arrange: IDs inexistentes — o GlobalExceptionHandler mapeia NotFoundException para 404
        var vm = new
        {
            ResiduoId = 99998,
            PontoColetaId = 99998,
            DataDescarte = DateTime.Now.AddMinutes(-1),
            Quantidade = 10.5m,
            Responsavel = "Operador"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/descartes", vm);

        // Assert — ponto/resíduo não existem: NotFoundException → 404
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
