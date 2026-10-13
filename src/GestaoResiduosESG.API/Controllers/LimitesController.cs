using Asp.Versioning;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.ViewModels.Limite;
using GestaoResiduosESG.Application.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoResiduosESG.API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/limites")]
[Route("api/limites")]
[Produces("application/json")]
public class LimitesController : ControllerBase
{
    private readonly ILimiteArmazenamentoService _service;

    public LimitesController(ILimiteArmazenamentoService service)
    {
        _service = service;
    }

    /// <summary>Lista limites de armazenamento com paginação</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseViewModel<LimiteResponseViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseViewModel<LimiteResponseViewModel>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Busca limite por ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LimiteResponseViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LimiteResponseViewModel>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Cria novo limite de armazenamento</summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(LimiteResponseViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LimiteResponseViewModel>> Create([FromBody] LimiteCreateViewModel vm)
    {
        var result = await _service.CreateAsync(vm);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Atualiza limite existente</summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] LimiteUpdateViewModel vm)
    {
        var result = await _service.UpdateAsync(id, vm);
        return result is null ? NotFound() : NoContent();
    }

    /// <summary>Remove limite de armazenamento</summary>
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
