using Asp.Versioning;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.Alerta;
using GestaoResiduosESG.Application.ViewModels.Shared;
using GestaoResiduosESG.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoResiduosESG.API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/alertas")]
[Route("api/alertas")]
[Produces("application/json")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaService _service;

    public AlertasController(IAlertaService service)
    {
        _service = service;
    }

    /// <summary>Lista alertas com paginação — filtro opcional por status (equivalente a VW_ALERTAS_PENDENTES)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseViewModel<AlertaResponseViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseViewModel<AlertaResponseViewModel>>> GetAll(
        [FromQuery] StatusAlerta? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Busca alerta por ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AlertaResponseViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertaResponseViewModel>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Resolve alerta (equivalente a SP_RESOLVER_ALERTA)</summary>
    [Authorize]
    [HttpPut("{id}/resolver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolver(int id, [FromBody] AlertaResolverViewModel vm)
    {
        var result = await _service.ResolverAsync(id, vm);
        return result is null ? NotFound() : NoContent();
    }
}
