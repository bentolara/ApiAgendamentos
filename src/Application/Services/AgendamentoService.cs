using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services;

public class AgendamentoService : IAgendamentoService
{
    private readonly ICalendarService _calendarService;
    private readonly IMapper _mapper;

    public AgendamentoService(ICalendarService calendarService, IMapper mapper)
    {
        _calendarService = calendarService;
        _mapper = mapper;
    }

    public async Task<AgendamentoDto> CriarAsync(CriarAgendamentoDto dto)
    {
        var agendamento = _mapper.Map<Agendamento>(dto);
        agendamento.Id = Guid.NewGuid();
        agendamento.DataCriacao = DateTime.UtcNow;

        // Verificar conflito de horário
        var existeConflito = await _calendarService.ExisteConflitoHorarioAsync(
            agendamento.DataHoraInicio,
            agendamento.DataHoraFim);

        if (existeConflito)
        {
            throw new DomainException("Já existe um agendamento neste horário.");
        }

        // Criar evento no Google Calendar
        var calendarEvent = await _calendarService.CreateEventAsync(agendamento);
        agendamento.GoogleCalendarEventId = calendarEvent.Id;

        return _mapper.Map<AgendamentoDto>(agendamento);
    }

    public async Task<IEnumerable<AgendamentoDto>> ObterTodosAsync()
    {
        // Buscar eventos dos últimos 30 dias e próximos 30 dias
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(30);
        
        var allAgendamentos = new List<Agendamento>();
        var processedEventIds = new HashSet<string>();
        
        // Buscar eventos por data, evitando duplicatas
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var events = await _calendarService.ListEventsAsync(date);
            foreach (var evt in events)
            {
                // Evitar processar o mesmo evento duas vezes (eventos que duram mais de um dia)
                if (processedEventIds.Contains(evt.Id))
                    continue;
                    
                processedEventIds.Add(evt.Id);
                
                var agendamento = MapEventToAgendamento(evt);
                if (agendamento != null)
                {
                    allAgendamentos.Add(agendamento);
                }
            }
        }

        return _mapper.Map<IEnumerable<AgendamentoDto>>(allAgendamentos);
    }

    public async Task<AgendamentoDto?> ObterPorIdAsync(Guid id)
    {
        // Buscar em um intervalo maior para encontrar o agendamento
        var startDate = DateTime.UtcNow.AddDays(-90);
        var endDate = DateTime.UtcNow.AddDays(90);
        
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var events = await _calendarService.ListEventsAsync(date);
            foreach (var evt in events)
            {
                if (evt.ExtendedProperties.TryGetValue("AgendamentoId", out var agendamentoIdStr) &&
                    Guid.TryParse(agendamentoIdStr, out var agendamentoId) &&
                    agendamentoId == id)
                {
                    var agendamento = MapEventToAgendamento(evt);
                    return agendamento != null ? _mapper.Map<AgendamentoDto>(agendamento) : null;
                }
            }
        }

        return null;
    }

    public async Task<AgendamentoDto> AtualizarAsync(Guid id, AtualizarAgendamentoDto dto)
    {
        // Buscar o agendamento existente
        var agendamentoDto = await ObterPorIdAsync(id);
        if (agendamentoDto == null)
        {
            throw new DomainException("Agendamento não encontrado.");
        }

        if (string.IsNullOrWhiteSpace(agendamentoDto.GoogleCalendarEventId))
        {
            throw new DomainException("Agendamento não possui evento no Google Calendar.");
        }

        // Verificar conflito de horário (excluindo o evento atual)
        var existeConflito = await _calendarService.ExisteConflitoHorarioAsync(
            dto.DataHoraInicio,
            dto.DataHoraFim,
            agendamentoDto.GoogleCalendarEventId);

        if (existeConflito)
        {
            throw new DomainException("Já existe um agendamento neste horário.");
        }

        // Mapear DTO para entidade
        var agendamento = _mapper.Map<Agendamento>(agendamentoDto);
        _mapper.Map(dto, agendamento);
        agendamento.DataAtualizacao = DateTime.UtcNow;

        // Atualizar evento no Google Calendar
        await _calendarService.UpdateEventAsync(agendamento.GoogleCalendarEventId!, agendamento);

        return _mapper.Map<AgendamentoDto>(agendamento);
    }

    public async Task<bool> DeletarAsync(Guid id)
    {
        // Buscar o agendamento
        var agendamentoDto = await ObterPorIdAsync(id);
        if (agendamentoDto == null)
        {
            throw new DomainException("Agendamento não encontrado.");
        }

        if (string.IsNullOrWhiteSpace(agendamentoDto.GoogleCalendarEventId))
        {
            throw new DomainException("Agendamento não possui evento no Google Calendar.");
        }

        // Deletar evento do Google Calendar
        return await _calendarService.DeleteEventAsync(agendamentoDto.GoogleCalendarEventId);
    }

    private Agendamento? MapEventToAgendamento(GoogleCalendarEvent evt)
    {
        try
        {

            if (!evt.ExtendedProperties.TryGetValue("AgendamentoId", out var agendamentoIdStr) ||
                !Guid.TryParse(agendamentoIdStr, out var agendamentoId))
            {
                evt.ExtendedProperties = new Dictionary<string, string>
                {
                    { "AgendamentoId", evt.Id.ToString() },
                    { "Description", evt.Description },

                };
                agendamentoId = Guid.NewGuid();
            }

            // Tentar deserializar dados completos da descrição
            AgendamentoData? data = null;
            if (!string.IsNullOrWhiteSpace(evt.Description))
            {
                try
                {
                    data = JsonSerializer.Deserialize<AgendamentoData>(evt.Description);
                }
                catch
                {
                    // Se não conseguir deserializar, usar dados dos ExtendedProperties
                    data = new AgendamentoData 
                    {
                        AgendamentoId = agendamentoIdStr,
                        ClienteNome = evt.ExtendedProperties.TryGetValue("ClienteNome", out var nome) ? nome : string.Empty,
                        ClienteTelefone = evt.ExtendedProperties.TryGetValue("ClienteTelefone", out var telefone) ? telefone : string.Empty,
                        DataHoraInicio = evt.Start,
                        DataHoraFim = evt.End,
                        Observacoes = $"{evt.Summary }",
                        DataCriacao = DateTime.UtcNow
                    };

                }
            }

            var agendamento = new Agendamento
            {
                Id = agendamentoId,
                GoogleCalendarEventId = evt.Id,
                DataHoraInicio = evt.Start,
                DataHoraFim = evt.End
            };

            if (data != null)
            {
                agendamento.ClienteNome = data.ClienteNome;
                agendamento.ClienteTelefone = data.ClienteTelefone;
                agendamento.Observacoes = data.Observacoes;
                agendamento.DataCriacao = data.DataCriacao;
                agendamento.DataAtualizacao = data.DataAtualizacao;
            }
            else
            {
                // Fallback para ExtendedProperties
                evt.ExtendedProperties.TryGetValue("ClienteNome", out var nome);
                evt.ExtendedProperties.TryGetValue("ClienteTelefone", out var telefone);
                
                agendamento.ClienteNome = nome ?? string.Empty;
                agendamento.ClienteTelefone = telefone ?? string.Empty;
                agendamento.DataCriacao = DateTime.UtcNow;
            }

            return agendamento;
        }
        catch
        {
            return null;
        }
    }

    private class AgendamentoData
    {
        public string AgendamentoId { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;
        public string ClienteTelefone { get; set; } = string.Empty;
        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }
        public string? Observacoes { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}

