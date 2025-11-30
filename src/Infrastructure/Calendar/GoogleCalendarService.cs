using Application.Interfaces;
using Domain.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Calendar;

public class GoogleCalendarService : ICalendarService
{
    private readonly CalendarService _calendarService;
    private readonly string _calendarId;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
    {
        _logger = logger;
        
        var credentialsPath = configuration["GoogleCalendar:CredentialsFile"];
        var calendarId = configuration["GoogleCalendar:CalendarId"];

        if (string.IsNullOrWhiteSpace(credentialsPath))
            throw new InvalidOperationException("GoogleCalendar:CredentialsFile não configurado no appsettings.json");

        if (string.IsNullOrWhiteSpace(calendarId))
            throw new InvalidOperationException("GoogleCalendar:CalendarId não configurado no appsettings.json");

        _calendarId = calendarId;

        try
        {
            // Carregar credenciais do service account
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }

            // Criar serviço do Google Calendar
            _calendarService = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "API de Agendamentos"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar Google Calendar Service");
            throw;
        }
    }

    public async Task<IList<GoogleCalendarEvent>> ListEventsAsync(DateTime date)
    {
        try
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var request = _calendarService.Events.List(_calendarId);
            request.TimeMin = startOfDay;
            request.TimeMax = endOfDay;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var response = await request.ExecuteAsync();

            return response.Items
                .Select(e => MapToGoogleCalendarEvent(e))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar eventos do Google Calendar para a data {Date}", date);
            throw;
        }
    }

    public async Task<GoogleCalendarEvent> CreateEventAsync(Agendamento agendamento)
    {
        try
        {
            var eventData = new Event
            {
                Summary = $"{agendamento.ClienteNome} - Agendamento",
                Description = SerializeAgendamentoData(agendamento),
                Start = new EventDateTime
                {
                    DateTime = agendamento.DataHoraInicio,
                    TimeZone = "America/Sao_Paulo"
                },
                End = new EventDateTime
                {
                    DateTime = agendamento.DataHoraFim,
                    TimeZone = "America/Sao_Paulo"
                },
                ExtendedProperties = new Event.ExtendedPropertiesData
                {
                    Private__ = new Dictionary<string, string>
                    {
                        { "AgendamentoId", agendamento.Id.ToString() },
                        { "ClienteNome", agendamento.ClienteNome },
                        { "ClienteTelefone", agendamento.ClienteTelefone },
                        { "CPF", ExtractCpfFromDescription(agendamento) ?? string.Empty }
                    }
                }
            };

            var request = _calendarService.Events.Insert(eventData, _calendarId);
            var createdEvent = await request.ExecuteAsync();

            _logger.LogInformation("Evento criado no Google Calendar: {EventId}", createdEvent.Id);

            return MapToGoogleCalendarEvent(createdEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar evento no Google Calendar para agendamento {AgendamentoId}", agendamento.Id);
            throw;
        }
    }

    public async Task<GoogleCalendarEvent> UpdateEventAsync(string eventId, Agendamento agendamento)
    {
        try
        {
            // Buscar evento existente
            var existingEvent = await _calendarService.Events.Get(_calendarId, eventId).ExecuteAsync();

            // Atualizar dados
            existingEvent.Summary = $"{agendamento.ClienteNome} - Agendamento";
            existingEvent.Description = SerializeAgendamentoData(agendamento);
            existingEvent.Start = new EventDateTime
            {
                DateTime = agendamento.DataHoraInicio,
                TimeZone = "America/Sao_Paulo"
            };
            existingEvent.End = new EventDateTime
            {
                DateTime = agendamento.DataHoraFim,
                TimeZone = "America/Sao_Paulo"
            };

            if (existingEvent.ExtendedProperties == null)
            {
                existingEvent.ExtendedProperties = new Event.ExtendedPropertiesData();
            }

            if (existingEvent.ExtendedProperties.Private__ == null)
            {
                existingEvent.ExtendedProperties.Private__ = new Dictionary<string, string>();
            }

            existingEvent.ExtendedProperties.Private__["AgendamentoId"] = agendamento.Id.ToString();
            existingEvent.ExtendedProperties.Private__["ClienteNome"] = agendamento.ClienteNome;
            existingEvent.ExtendedProperties.Private__["ClienteTelefone"] = agendamento.ClienteTelefone;
            existingEvent.ExtendedProperties.Private__["CPF"] = ExtractCpfFromDescription(agendamento) ?? string.Empty;

            var request = _calendarService.Events.Update(existingEvent, _calendarId, eventId);
            var updatedEvent = await request.ExecuteAsync();

            _logger.LogInformation("Evento atualizado no Google Calendar: {EventId}", updatedEvent.Id);

            return MapToGoogleCalendarEvent(updatedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar evento {EventId} no Google Calendar", eventId);
            throw;
        }
    }

    public async Task<bool> DeleteEventAsync(string eventId)
    {
        try
        {
            await _calendarService.Events.Delete(_calendarId, eventId).ExecuteAsync();
            _logger.LogInformation("Evento deletado do Google Calendar: {EventId}", eventId);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Evento {EventId} não encontrado no Google Calendar", eventId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar evento {EventId} do Google Calendar", eventId);
            throw;
        }
    }

    public async Task<GoogleCalendarEvent?> GetEventByCpfAndDateAsync(string cpf, DateTime date)
    {
        try
        {
            var events = await ListEventsAsync(date);
            
            foreach (var evt in events)
            {
                if (evt.ExtendedProperties.TryGetValue("CPF", out var eventCpf) && 
                    eventCpf.Equals(cpf, StringComparison.OrdinalIgnoreCase))
                {
                    return evt;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evento por CPF {CPF} e data {Date}", cpf, date);
            throw;
        }
    }

    public async Task<bool> ExisteConflitoHorarioAsync(DateTime dataHoraInicio, DateTime dataHoraFim, string? eventIdExcluir = null)
    {
        try
        {
            var startDate = dataHoraInicio.Date;
            var endDate = dataHoraFim.Date;

            // Buscar eventos em todas as datas do intervalo
            var allDates = new List<DateTime>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                allDates.Add(date);
            }

            var allEvents = new List<GoogleCalendarEvent>();
            foreach (var date in allDates)
            {
                var events = await ListEventsAsync(date);
                allEvents.AddRange(events);
            }

            // Verificar conflitos
            foreach (var evt in allEvents)
            {
                if (eventIdExcluir != null && evt.Id == eventIdExcluir)
                    continue;

                // Verificar sobreposição de horários
                if ((dataHoraInicio >= evt.Start && dataHoraInicio < evt.End) ||
                    (dataHoraFim > evt.Start && dataHoraFim <= evt.End) ||
                    (dataHoraInicio <= evt.Start && dataHoraFim >= evt.End))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar conflito de horário");
            throw;
        }
    }

    public async Task<GoogleCalendarEvent?> GetEventByIdAsync(string eventId)
    {
        try
        {
            var request = _calendarService.Events.Get(_calendarId, eventId);
            var eventData = await request.ExecuteAsync();
            return MapToGoogleCalendarEvent(eventData);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Evento {EventId} não encontrado no Google Calendar", eventId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evento {EventId} no Google Calendar", eventId);
            throw;
        }
    }

    private GoogleCalendarEvent MapToGoogleCalendarEvent(Event eventData)
    {
        var evt = new GoogleCalendarEvent
        {
            Id = eventData.Id ?? string.Empty,
            Summary = eventData.Summary ?? string.Empty,
            Description = eventData.Description ?? string.Empty
        };

        if (eventData.Start?.DateTime != null)
        {
            evt.Start = eventData.Start.DateTime.Value;
        }
        else if (eventData.Start?.Date != null)
        {
            evt.Start = DateTime.Parse(eventData.Start.Date);
        }

        if (eventData.End?.DateTime != null)
        {
            evt.End = eventData.End.DateTime.Value;
        }
        else if (eventData.End?.Date != null)
        {
            evt.End = DateTime.Parse(eventData.End.Date);
        }

        if (eventData.ExtendedProperties?.Private__ != null)
        {
            evt.ExtendedProperties = new Dictionary<string, string>(eventData.ExtendedProperties.Private__);
        }

        return evt;
    }

    private string SerializeAgendamentoData(Agendamento agendamento)
    {
        var data = new
        {
            AgendamentoId = agendamento.Id.ToString(),
            ClienteNome = agendamento.ClienteNome,
            ClienteTelefone = agendamento.ClienteTelefone,
            DataHoraInicio = agendamento.DataHoraInicio,
            DataHoraFim = agendamento.DataHoraFim,
            Observacoes = agendamento.Observacoes,
            DataCriacao = agendamento.DataCriacao,
            DataAtualizacao = agendamento.DataAtualizacao
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private string? ExtractCpfFromDescription(Agendamento agendamento)
    {
        // Se houver CPF no telefone ou em outro campo, extrair aqui
        // Por enquanto, retornar null se não houver campo específico
        // Você pode adicionar um campo CPF à entidade Agendamento se necessário
        return null;
    }
}

