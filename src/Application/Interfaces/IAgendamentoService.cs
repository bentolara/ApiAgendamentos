using Application.DTOs;

namespace Application.Interfaces;

public interface IAgendamentoService
{
    Task<AgendamentoDto> CriarAsync(CriarAgendamentoDto dto);
    Task<IEnumerable<AgendamentoDto>> ObterTodosAsync();
    Task<AgendamentoDto?> ObterPorIdAsync(Guid id);
    Task<AgendamentoDto> AtualizarAsync(Guid id, AtualizarAgendamentoDto dto);
    Task<bool> DeletarAsync(Guid id);
}

