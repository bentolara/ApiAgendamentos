using Domain.Entities;

namespace Application.Interfaces;

public interface ICalendarService
{
    /// <summary>
    /// Lista eventos do Google Calendar para uma data específica
    /// </summary>
    Task<IList<GoogleCalendarEvent>> ListEventsAsync(DateTime date);

    /// <summary>
    /// Cria um novo evento no Google Calendar a partir de um Agendamento
    /// </summary>
    Task<GoogleCalendarEvent> CreateEventAsync(Agendamento agendamento);

    /// <summary>
    /// Atualiza um evento existente no Google Calendar
    /// </summary>
    Task<GoogleCalendarEvent> UpdateEventAsync(string eventId, Agendamento agendamento);

    /// <summary>
    /// Deleta um evento do Google Calendar
    /// </summary>
    Task<bool> DeleteEventAsync(string eventId);

    /// <summary>
    /// Busca um evento por CPF e data
    /// </summary>
    Task<GoogleCalendarEvent?> GetEventByCpfAndDateAsync(string cpf, DateTime date);

    /// <summary>
    /// Verifica se existe conflito de horário no intervalo especificado
    /// </summary>
    Task<bool> ExisteConflitoHorarioAsync(DateTime dataHoraInicio, DateTime dataHoraFim, string? eventIdExcluir = null);

    /// <summary>
    /// Busca um evento por ID do Google Calendar
    /// </summary>
    Task<GoogleCalendarEvent?> GetEventByIdAsync(string eventId);
}

/// <summary>
/// Representa um evento do Google Calendar
/// </summary>
public class GoogleCalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Dictionary<string, string> ExtendedProperties { get; set; } = new();
}

