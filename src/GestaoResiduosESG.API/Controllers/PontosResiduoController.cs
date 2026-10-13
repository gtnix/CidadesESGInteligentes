using Asp.Versioning;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.PontoResiduo;
using GestaoResiduosESG.Application.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoResiduosESG.API.Controllers;

/// <summary>Gerencia os resíduos aceitos por um ponto de coleta (vínculo N:N)</summary>
[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/pontos-coleta/{pontoId:int}/residuos")]
[Route("api/pontos-coleta/{pontoId:int}/residuos")]
[Produces("application/json")]
public class PontosResiduoController : ControllerBase
{
    private readonly IPontoResiduoService _service;

    public PontosResiduoController(IPontoResiduoService service)
    {
        _service = service;
    }

    /// <summary>Lista os resíduos aceitos por um ponto de coleta com paginação</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseViewModel<PontoResiduoResponseViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponseViewModel<PontoResiduoResponseViewModel>>> GetByPonto(
        int pontoId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetByPontoColetaAsync(pontoId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Vincula um resíduo a um ponto de coleta (define quais tipos são aceitos)</summary>
    [Authorize]
    [HttpPost("{residuoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Vincular(int pontoId, int residuoId)
    {
        await _service.VincularAsync(pontoId, residuoId);
        return NoContent();
    }

    /// <summary>Remove o vínculo entre um resíduo e um ponto de coleta</summary>
    [Authorize]
    [HttpDelete("{residuoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desvincular(int pontoId, int residuoId)
    {
        await _service.DesvincularAsync(pontoId, residuoId);
        return NoContent();
    }
}
