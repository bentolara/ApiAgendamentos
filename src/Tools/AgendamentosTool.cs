using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using FluentValidation;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoogleCalendarEvent = Application.Interfaces.GoogleCalendarEvent;

namespace Tools;

public static class AgendamentosTool
{
    public static ToolDefinition AgendarCriarTool => new ToolDefinition
    {
        Name = "agendar_criar",
        Description = "Cria um novo agendamento no Google Calendar. Valida os dados de entrada e verifica conflitos de horário antes de criar.",
        InputSchema = new JsonSchema
        {
            Type = "object",
            Properties = new Dictionary<string, JsonSchema>
            {
                ["clienteNome"] = new JsonSchema
                {
                    Type = "string",
                    Description = "Nome completo do cliente (obrigatório, máximo 200 caracteres)"
                },
                ["clienteTelefone"] = new JsonSchema
                {
                    Type = "string",
                    Description = "Telefone do cliente (obrigatório, máximo 20 caracteres)"
                },
                ["dataHoraInicio"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Description = "Data e hora de início do agendamento (formato ISO 8601, deve ser futura)"
                },
                ["dataHoraFim"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Description = "Data e hora de fim do agendamento (formato ISO 8601, deve ser futura e posterior à data de início)"
                },
                ["observacoes"] = new JsonSchema
                {
                    Type = "string",
                    Description = "Observações adicionais sobre o agendamento (opcional, máximo 1000 caracteres)"
                }
            },
            Required = new[] { "clienteNome", "clienteTelefone", "dataHoraInicio", "dataHoraFim" }
        }
    };

    public static ToolDefinition AgendarConsultarTool => new ToolDefinition
    {
        Name = "agendar_consultar",
        Description = "Consulta agendamentos existentes. Pode filtrar por data, hora ou retornar todos os agendamentos.",
        InputSchema = new JsonSchema
        {
            Type = "object",
            Properties = new Dictionary<string, JsonSchema>
            {
                ["data"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date",
                    Description = "Data para consultar agendamentos (formato YYYY-MM-DD, opcional)"
                },
                ["dataInicio"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date",
                    Description = "Data inicial do intervalo de consulta (opcional)"
                },
                ["dataFim"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date",
                    Description = "Data final do intervalo de consulta (opcional)"
                }
            }
        }
    };

    public static ToolDefinition AgendarAtualizarTool => new ToolDefinition
    {
        Name = "agendar_atualizar",
        Description = "Atualiza um agendamento existente. Pode ser identificado por ID do agendamento ou pelo eventId do Google Calendar.",
        InputSchema = new JsonSchema
        {
            Type = "object",
            Properties = new Dictionary<string, JsonSchema>
            {
                ["id"] = new JsonSchema
                {
                    Type = "string",
                    Format = "uuid",
                    Description = "ID do agendamento (UUID)"
                },
                ["eventId"] = new JsonSchema
                {
                    Type = "string",
                    Description = "ID do evento no Google Calendar (alternativa ao id)"
                },
                ["clienteNome"] = new JsonSchema
                {
                    Type = "string",
                    Description = "Novo nome do cliente (obrigatório)"
                },
                ["clienteTelefone"] = new JsonSchema
                {
                    Type = "string",
                    Description = "Novo telefone do cliente (obrigatório)"
                },
                ["dataHoraInicio"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Description = "Nova data e hora de início (obrigatório)"
                },
                ["dataHoraFim"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Description = "Nova data e hora de fim (obrigatório)"
                },
                ["observacoes"] = new JsonSchema
                {
                    Type = "string",
                    Description = "Novas observações (opcional)"
                }
            },
            Required = new[] { "clienteNome", "clienteTelefone", "dataHoraInicio", "dataHoraFim" }
        }
    };

    public static ToolDefinition AgendarDeletarTool => new ToolDefinition
    {
        Name = "agendar_deletar",
        Description = "Deleta um agendamento existente. Pode ser identificado por ID do agendamento ou pelo eventId do Google Calendar.",
        InputSchema = new JsonSchema
        {
            Type = "object",
            Properties = new Dictionary<string, JsonSchema>
            {
                ["id"] = new JsonSchema
                {
                    Type = "string",
                    Format = "uuid",
                    Description = "ID do agendamento (UUID)"
                },
                ["eventId"] = new JsonSchema
                {
                    Type = "string",
                    Description = "ID do evento no Google Calendar (alternativa ao id)"
                }
            },
            Required = new[] { "id" }
        }
    };

    public static ToolDefinition AgendarDisponibilidadeTool => new ToolDefinition
    {
        Name = "agendar_disponibilidade",
        Description = "Verifica a disponibilidade de um horário específico. Retorna se o horário está livre ou ocupado, e detalhes do agendamento existente se houver conflito.",
        InputSchema = new JsonSchema
        {
            Type = "object",
            Properties = new Dictionary<string, JsonSchema>
            {
                ["dataHoraInicio"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Description = "Data e hora de início para verificar disponibilidade (obrigatório)"
                },
                ["dataHoraFim"] = new JsonSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Description = "Data e hora de fim para verificar disponibilidade (obrigatório)"
                }
            },
            Required = new[] { "dataHoraInicio", "dataHoraFim" }
        }
    };

    public static async Task<ToolResult> ExecuteAgendarCriar(
        JsonElement arguments,
        IAgendamentoService agendamentoService,
        IValidator<CriarAgendamentoDto> validator)
    {
        try
        {
            var dto = JsonSerializer.Deserialize<CriarAgendamentoDto>(
                arguments.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto == null)
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = new[] { "Dados de entrada inválidos" },
                    Content = null
                };
            }

            // Validar com FluentValidation
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray(),
                    Content = null
                };
            }

            var agendamento = await agendamentoService.CriarAsync(dto);

            return new ToolResult
            {
                Success = true,
                Errors = Array.Empty<string>(),
                Content = new Dictionary<string, object?>
                {
                    ["agendamento"] = new
                    {
                        id = agendamento.Id.ToString(),
                        clienteNome = agendamento.ClienteNome,
                        clienteTelefone = agendamento.ClienteTelefone,
                        dataHoraInicio = agendamento.DataHoraInicio.ToString("O"),
                        dataHoraFim = agendamento.DataHoraFim.ToString("O"),
                        observacoes = agendamento.Observacoes,
                        dataCriacao = agendamento.DataCriacao.ToString("O"),
                        googleCalendarEventId = agendamento.GoogleCalendarEventId
                    }
                }
            };
        }
        catch (DomainException ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { ex.Message },
                Content = null
            };
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { $"Erro inesperado: {ex.Message}" },
                Content = null
            };
        }
    }

    public static async Task<ToolResult> ExecuteAgendarConsultar(
        JsonElement arguments,
        IAgendamentoService agendamentoService,
        ICalendarService calendarService)
    {
        try
        {
            var dataStr = arguments.TryGetProperty("data", out var dataProp) ? dataProp.GetString() : null;
            var dataInicioStr = arguments.TryGetProperty("dataInicio", out var inicioProp) ? inicioProp.GetString() : null;
            var dataFimStr = arguments.TryGetProperty("dataFim", out var fimProp) ? fimProp.GetString() : null;

            IEnumerable<AgendamentoDto> agendamentos;

            if (!string.IsNullOrWhiteSpace(dataStr) && DateTime.TryParse(dataStr, out var data))
            {
                // Consultar por data específica
                var events = await calendarService.ListEventsAsync(data);
                agendamentos = events
                    .Where(e => e.ExtendedProperties.ContainsKey("AgendamentoId"))
                    .Select(e => MapEventToDto(e))
                    .Where(d => d != null)
                    .Cast<AgendamentoDto>();
            }
            else if (!string.IsNullOrWhiteSpace(dataInicioStr) && !string.IsNullOrWhiteSpace(dataFimStr) &&
                     DateTime.TryParse(dataInicioStr, out var inicio) &&
                     DateTime.TryParse(dataFimStr, out var fim))
            {
                // Consultar por intervalo
                var allAgendamentos = new List<AgendamentoDto>();
                for (var date = inicio.Date; date <= fim.Date; date = date.AddDays(1))
                {
                    var events = await calendarService.ListEventsAsync(date);
                    var dtos = events
                        .Where(e => e.ExtendedProperties.ContainsKey("AgendamentoId"))
                        .Select(e => MapEventToDto(e))
                        .Where(d => d != null)
                        .Cast<AgendamentoDto>();
                    allAgendamentos.AddRange(dtos);
                }
                agendamentos = allAgendamentos.DistinctBy(a => a.Id);
            }
            else
            {
                // Retornar todos
                agendamentos = await agendamentoService.ObterTodosAsync();
            }

            return new ToolResult
            {
                Success = true,
                Errors = Array.Empty<string>(),
                Content = new Dictionary<string, object?>
                {
                    ["agendamentos"] = agendamentos.Select(a => new
                    {
                        id = a.Id.ToString(),
                        clienteNome = a.ClienteNome,
                        clienteTelefone = a.ClienteTelefone,
                        dataHoraInicio = a.DataHoraInicio.ToString("O"),
                        dataHoraFim = a.DataHoraFim.ToString("O"),
                        observacoes = a.Observacoes,
                        dataCriacao = a.DataCriacao.ToString("O"),
                        dataAtualizacao = a.DataAtualizacao?.ToString("O"),
                        googleCalendarEventId = a.GoogleCalendarEventId
                    }).ToArray()
                }
            };
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { $"Erro ao consultar agendamentos: {ex.Message}" },
                Content = null
            };
        }
    }

    public static async Task<ToolResult> ExecuteAgendarAtualizar(
        JsonElement arguments,
        IAgendamentoService agendamentoService,
        IValidator<AtualizarAgendamentoDto> validator,
        ICalendarService calendarService)
    {
        try
        {
            Guid? id = null;
            if (arguments.TryGetProperty("id", out var idProp))
            {
                if (Guid.TryParse(idProp.GetString(), out var parsedId))
                {
                    id = parsedId;
                }
            }

            string? eventId = null;
            if (arguments.TryGetProperty("eventId", out var eventIdProp))
            {
                eventId = eventIdProp.GetString();
            }

            if (!id.HasValue && string.IsNullOrWhiteSpace(eventId))
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = new[] { "É necessário fornecer 'id' ou 'eventId' para atualizar o agendamento" },
                    Content = null
                };
            }

            // Se forneceu eventId, buscar o agendamento por ele
            if (!id.HasValue && !string.IsNullOrWhiteSpace(eventId))
            {
                var eventData = await calendarService.GetEventByIdAsync(eventId);
                if (eventData?.ExtendedProperties.TryGetValue("AgendamentoId", out var agendamentoIdStr) == true &&
                    Guid.TryParse(agendamentoIdStr, out var agendamentoId))
                {
                    id = agendamentoId;
                }
                else
                {
                    return new ToolResult
                    {
                        Success = false,
                        Errors = new[] { "Evento não encontrado no Google Calendar" },
                        Content = null
                    };
                }
            }

            var dto = JsonSerializer.Deserialize<AtualizarAgendamentoDto>(
                arguments.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto == null)
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = new[] { "Dados de entrada inválidos" },
                    Content = null
                };
            }

            // Validar com FluentValidation
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray(),
                    Content = null
                };
            }

            var agendamento = await agendamentoService.AtualizarAsync(id!.Value, dto);

            return new ToolResult
            {
                Success = true,
                Errors = Array.Empty<string>(),
                Content = new Dictionary<string, object?>
                {
                    ["agendamento"] = new
                    {
                        id = agendamento.Id.ToString(),
                        clienteNome = agendamento.ClienteNome,
                        clienteTelefone = agendamento.ClienteTelefone,
                        dataHoraInicio = agendamento.DataHoraInicio.ToString("O"),
                        dataHoraFim = agendamento.DataHoraFim.ToString("O"),
                        observacoes = agendamento.Observacoes,
                        dataCriacao = agendamento.DataCriacao.ToString("O"),
                        dataAtualizacao = agendamento.DataAtualizacao?.ToString("O"),
                        googleCalendarEventId = agendamento.GoogleCalendarEventId
                    }
                }
            };
        }
        catch (DomainException ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { ex.Message },
                Content = null
            };
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { $"Erro inesperado: {ex.Message}" },
                Content = null
            };
        }
    }

    public static async Task<ToolResult> ExecuteAgendarDeletar(
        JsonElement arguments,
        IAgendamentoService agendamentoService,
        ICalendarService calendarService)
    {
        try
        {
            Guid? id = null;
            if (arguments.TryGetProperty("id", out var idProp))
            {
                if (Guid.TryParse(idProp.GetString(), out var parsedId))
                {
                    id = parsedId;
                }
            }

            string? eventId = null;
            if (arguments.TryGetProperty("eventId", out var eventIdProp))
            {
                eventId = eventIdProp.GetString();
            }

            if (!id.HasValue && string.IsNullOrWhiteSpace(eventId))
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = new[] { "É necessário fornecer 'id' ou 'eventId' para deletar o agendamento" },
                    Content = null
                };
            }

            // Se forneceu eventId, buscar o agendamento por ele
            if (!id.HasValue && !string.IsNullOrWhiteSpace(eventId))
            {
                var eventData = await calendarService.GetEventByIdAsync(eventId);
                if (eventData?.ExtendedProperties.TryGetValue("AgendamentoId", out var agendamentoIdStr) == true &&
                    Guid.TryParse(agendamentoIdStr, out var agendamentoId))
                {
                    id = agendamentoId;
                }
                else
                {
                    return new ToolResult
                    {
                        Success = false,
                        Errors = new[] { "Evento não encontrado no Google Calendar" },
                        Content = null
                    };
                }
            }

            var deletado = await agendamentoService.DeletarAsync(id!.Value);

            return new ToolResult
            {
                Success = deletado,
                Errors = deletado ? Array.Empty<string>() : new[] { "Não foi possível deletar o agendamento" },
                Content = new Dictionary<string, object?>
                {
                    ["deletado"] = deletado,
                    ["mensagem"] = deletado ? "Agendamento deletado com sucesso" : "Falha ao deletar agendamento"
                }
            };
        }
        catch (DomainException ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { ex.Message },
                Content = null
            };
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { $"Erro inesperado: {ex.Message}" },
                Content = null
            };
        }
    }

    public static async Task<ToolResult> ExecuteAgendarDisponibilidade(
        JsonElement arguments,
        ICalendarService calendarService)
    {
        try
        {
            if (!arguments.TryGetProperty("dataHoraInicio", out var inicioProp) ||
                !arguments.TryGetProperty("dataHoraFim", out var fimProp))
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = new[] { "É necessário fornecer 'dataHoraInicio' e 'dataHoraFim'" },
                    Content = null
                };
            }

            if (!DateTime.TryParse(inicioProp.GetString(), out var dataHoraInicio) ||
                !DateTime.TryParse(fimProp.GetString(), out var dataHoraFim))
            {
                return new ToolResult
                {
                    Success = false,
                    Errors = new[] { "Formato de data/hora inválido. Use formato ISO 8601" },
                    Content = null
                };
            }

            var existeConflito = await calendarService.ExisteConflitoHorarioAsync(dataHoraInicio, dataHoraFim);

            if (existeConflito)
            {
                // Buscar o agendamento conflitante
                var startDate = dataHoraInicio.Date;
                var endDate = dataHoraFim.Date;
                var agendamentoConflitante = (AgendamentoDto?)null;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var events = await calendarService.ListEventsAsync(date);
                    foreach (var evt in events)
                    {
                        if ((dataHoraInicio >= evt.Start && dataHoraInicio < evt.End) ||
                            (dataHoraFim > evt.Start && dataHoraFim <= evt.End) ||
                            (dataHoraInicio <= evt.Start && dataHoraFim >= evt.End))
                        {
                            agendamentoConflitante = MapEventToDto(evt);
                            break;
                        }
                    }
                    if (agendamentoConflitante != null) break;
                }

                return new ToolResult
                {
                    Success = true,
                    Errors = Array.Empty<string>(),
                    Content = new Dictionary<string, object?>
                    {
                        ["disponivel"] = false,
                        ["mensagem"] = "Horário ocupado",
                        ["agendamentoConflitante"] = agendamentoConflitante != null ? new
                        {
                            id = agendamentoConflitante.Id.ToString(),
                            clienteNome = agendamentoConflitante.ClienteNome,
                            dataHoraInicio = agendamentoConflitante.DataHoraInicio.ToString("O"),
                            dataHoraFim = agendamentoConflitante.DataHoraFim.ToString("O")
                        } : null
                    }
                };
            }

            return new ToolResult
            {
                Success = true,
                Errors = Array.Empty<string>(),
                Content = new Dictionary<string, object?>
                {
                    ["disponivel"] = true,
                    ["mensagem"] = "Horário disponível"
                }
            };
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                Success = false,
                Errors = new[] { $"Erro ao verificar disponibilidade: {ex.Message}" },
                Content = null
            };
        }
    }

    private static AgendamentoDto? MapEventToDto(GoogleCalendarEvent evt)
    {
        try
        {
            if (!evt.ExtendedProperties.TryGetValue("AgendamentoId", out var agendamentoIdStr) ||
                !Guid.TryParse(agendamentoIdStr, out var agendamentoId))
            {
                return null;
            }

            evt.ExtendedProperties.TryGetValue("ClienteNome", out var nome);
            evt.ExtendedProperties.TryGetValue("ClienteTelefone", out var telefone);

            return new AgendamentoDto
            {
                Id = agendamentoId,
                ClienteNome = nome ?? string.Empty,
                ClienteTelefone = telefone ?? string.Empty,
                DataHoraInicio = evt.Start,
                DataHoraFim = evt.End,
                GoogleCalendarEventId = evt.Id,
                DataCriacao = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }
}

// Modelos para MCP
public class ToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JsonSchema InputSchema { get; set; } = new();
}

public class JsonSchema
{
    public string Type { get; set; } = string.Empty;
    public string? Format { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, JsonSchema>? Properties { get; set; }
    public string[]? Required { get; set; }
}

public class ToolResult
{
    public bool Success { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();
    public Dictionary<string, object?>? Content { get; set; }
}

