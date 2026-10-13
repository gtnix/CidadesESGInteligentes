using Asp.Versioning;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.Descarte;
using GestaoResiduosESG.Application.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoResiduosESG.API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/descartes")]
[Route("api/descartes")]
[Produces("application/json")]
public class DescartesController : ControllerBase
{
    private readonly IDescarteService _service;

    public DescartesController(IDescarteService service)
    {
        _service = service;
    }

    /// <summary>Lista descartes com paginação</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseViewModel<DescarteResponseViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseViewModel<DescarteResponseViewModel>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Busca descarte por ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DescarteResponseViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DescarteResponseViewModel>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Registra novo descarte — valida resíduo aceito, atualiza quantidade e gera alertas automaticamente</summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(DescarteResponseViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DescarteResponseViewModel>> Create([FromBody] DescarteCreateViewModel vm)
    {
        var result = await _service.CreateAsync(vm);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Remove descarte</summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
