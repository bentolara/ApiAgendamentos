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
    private readonly IAgendamentoRepository _repository;
    private readonly IMapper _mapper;

    public AgendamentoService(IAgendamentoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AgendamentoDto> CriarAsync(CriarAgendamentoDto dto)
    {
        var agendamento = _mapper.Map<Agendamento>(dto);
        agendamento.Id = Guid.NewGuid();
        agendamento.DataCriacao = DateTime.UtcNow;

        var existeConflito = await _repository.ExisteConflitoHorarioAsync(
            agendamento.DataHoraInicio,
            agendamento.DataHoraFim);

        if (existeConflito)
        {
            throw new DomainException("Já existe um agendamento neste horário.");
        }

        var resultado = await _repository.AddAsync(agendamento);
        return _mapper.Map<AgendamentoDto>(resultado);
    }

    public async Task<IEnumerable<AgendamentoDto>> ObterTodosAsync()
    {
        var agendamentos = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<AgendamentoDto>>(agendamentos);
    }

    public async Task<AgendamentoDto?> ObterPorIdAsync(Guid id)
    {
        var agendamento = await _repository.GetByIdAsync(id);
        return agendamento == null ? null : _mapper.Map<AgendamentoDto>(agendamento);
    }

    public async Task<AgendamentoDto> AtualizarAsync(Guid id, AtualizarAgendamentoDto dto)
    {
        var agendamento = await _repository.GetByIdAsync(id);
        if (agendamento == null)
        {
            throw new DomainException("Agendamento não encontrado.");
        }

        var existeConflito = await _repository.ExisteConflitoHorarioAsync(
            dto.DataHoraInicio,
            dto.DataHoraFim,
            id);

        if (existeConflito)
        {
            throw new DomainException("Já existe um agendamento neste horário.");
        }

        _mapper.Map(dto, agendamento);
        agendamento.DataAtualizacao = DateTime.UtcNow;

        var resultado = await _repository.UpdateAsync(agendamento);
        return _mapper.Map<AgendamentoDto>(resultado);
    }

    public async Task<bool> DeletarAsync(Guid id)
    {
        var existe = await _repository.ExistsAsync(id);
        if (!existe)
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
                        Observacoes = $"{evt.Summary}",
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
    /*private Agendamento? MapEventToAgendamento(GoogleCalendarEvent evt)
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
    }*/

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

