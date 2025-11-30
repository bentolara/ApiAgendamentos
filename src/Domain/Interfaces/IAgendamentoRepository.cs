using Domain.Entities;

namespace Domain.Interfaces;

public interface IAgendamentoRepository : IRepository<Agendamento>
{
    Task<bool> ExisteConflitoHorarioAsync(DateTime dataHoraInicio, DateTime dataHoraFim, Guid? idExcluir = null);
}

