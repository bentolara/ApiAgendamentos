using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendamentosController : ControllerBase
{
    private readonly IAgendamentoService _agendamentoService;
    private readonly ILogger<AgendamentosController> _logger;

    public AgendamentosController(
        IAgendamentoService agendamentoService,
        ILogger<AgendamentosController> logger)
    {
        _agendamentoService = agendamentoService;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo agendamento
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AgendamentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgendamentoDto>> Criar([FromBody] CriarAgendamentoDto dto)
    {
        try
        {
            var agendamento = await _agendamentoService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = agendamento.Id }, agendamento);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Erro de domínio ao criar agendamento");
            return BadRequest(new { error = true, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todos os agendamentos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AgendamentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AgendamentoDto>>> ObterTodos()
    {
        var agendamentos = await _agendamentoService.ObterTodosAsync();
        return Ok(agendamentos);
    }

    /// <summary>
    /// Obtém um agendamento por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AgendamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendamentoDto>> ObterPorId(Guid id)
    {
        var agendamento = await _agendamentoService.ObterPorIdAsync(id);
        
        if (agendamento == null)
        {
            return NotFound(new { error = true, message = "Agendamento não encontrado." });
        }

        return Ok(agendamento);
    }

    /// <summary>
    /// Atualiza um agendamento existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AgendamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendamentoDto>> Atualizar(Guid id, [FromBody] AtualizarAgendamentoDto dto)
    {
        try
        {
            var agendamento = await _agendamentoService.AtualizarAsync(id, dto);
            return Ok(agendamento);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Erro de domínio ao atualizar agendamento {Id}", id);
            
            if (ex.Message.Contains("não encontrado"))
            {
                return NotFound(new { error = true, message = ex.Message });
            }
            
            return BadRequest(new { error = true, message = ex.Message });
        }
    }

    /// <summary>
    /// Deleta um agendamento
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id)
    {
        try
        {
            var deletado = await _agendamentoService.DeletarAsync(id);
            
            if (!deletado)
            {
                return NotFound(new { error = true, message = "Agendamento não encontrado." });
            }

            return NoContent();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Erro de domínio ao deletar agendamento {Id}", id);
            return NotFound(new { error = true, message = ex.Message });
        }
    }
}

