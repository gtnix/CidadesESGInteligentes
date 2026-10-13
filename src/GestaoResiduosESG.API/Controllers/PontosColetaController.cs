using Asp.Versioning;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using GestaoResiduosESG.Application.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoResiduosESG.API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/pontos-coleta")]
[Route("api/pontos-coleta")]
[Produces("application/json")]
public class PontosColetaController : ControllerBase
{
    private readonly IPontoColetaService _service;

    public PontosColetaController(IPontoColetaService service)
    {
        _service = service;
    }

    /// <summary>Lista pontos de coleta com paginação</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseViewModel<PontoColetaResponseViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseViewModel<PontoColetaResponseViewModel>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Busca ponto de coleta por ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PontoColetaResponseViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PontoColetaResponseViewModel>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Retorna status de ocupação do ponto (equivalente a VW_STATUS_PONTOS_COLETA)</summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(PontoColetaStatusViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PontoColetaStatusViewModel>> GetStatus(int id)
    {
        var result = await _service.GetStatusAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Cria novo ponto de coleta</summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(PontoColetaResponseViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PontoColetaResponseViewModel>> Create([FromBody] PontoColetaCreateViewModel vm)
    {
        var result = await _service.CreateAsync(vm);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Atualiza ponto de coleta existente</summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] PontoColetaUpdateViewModel vm)
    {
        var result = await _service.UpdateAsync(id, vm);
        return result is null ? NotFound() : NoContent();
    }

    /// <summary>Remove ponto de coleta</summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Registra coleta no ponto — zera quantidade e resolve alertas (equivalente a SP_REGISTRAR_COLETA)</summary>
    [Authorize]
    [HttpPost("{id}/registrar-coleta")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarColeta(int id)
    {
        var success = await _service.RegistrarColetaAsync(id);
        return success ? NoContent() : NotFound();
    }
}
