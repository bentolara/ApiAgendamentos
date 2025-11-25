using Application.DTOs;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Infrastructure.Calendar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tools;

namespace McpServer;

public class McpServer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<McpServer> _logger;

    public McpServer(IServiceProvider serviceProvider, ILogger<McpServer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // Registrar todas as ferramentas
        McpToolRegistry.RegisterAllTools();

        _logger.LogInformation("Servidor MCP iniciado. Aguardando requisições via STDIO...");

        // Ler do stdin e escrever no stdout
        using var reader = new StreamReader(Console.OpenStandardInput());
        using var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var request = JsonSerializer.Deserialize<McpRequest>(line);
                if (request == null)
                {
                    await SendErrorResponse(writer, "Invalid request format");
                    continue;
                }

                var response = await HandleRequest(request);
                var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await writer.WriteLineAsync(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar requisição");
                await SendErrorResponse(writer, $"Erro interno: {ex.Message}");
            }
        }
    }

    private async Task<McpResponse> HandleRequest(McpRequest request)
    {
        switch (request.Method)
        {
            case "tools/list":
                return HandleToolsList();

            case "tools/call":
                return await HandleToolCall(request);

            case "initialize":
                return HandleInitialize();

            default:
                return new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError { Message = $"Método não suportado: {request.Method}" }
                };
        }
    }

    private McpResponse HandleInitialize()
    {
        return new McpResponse
        {
            Result = new Dictionary<string, object?>
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new Dictionary<string, object?>
                {
                    ["tools"] = new Dictionary<string, object?>
                    {
                        ["listChanged"] = true
                    }
                },
                ["serverInfo"] = new Dictionary<string, object?>
                {
                    ["name"] = "agendamentos-mcp-server",
                    ["version"] = "1.0.0"
                }
            }
        };
    }

    private McpResponse HandleToolsList()
    {
        var tools = McpToolRegistry.GetTools();
        var toolsList = tools.Select(t => new Dictionary<string, object?>
        {
            ["name"] = t.Name,
            ["description"] = t.Description,
            ["inputSchema"] = ConvertJsonSchema(t.InputSchema)
        }).ToArray();

        return new McpResponse
        {
            Result = new Dictionary<string, object?>
            {
                ["tools"] = toolsList
            }
        };
    }

    private async Task<McpResponse> HandleToolCall(McpRequest request)
    {
        if (request.Params == null || !request.Params.TryGetValue("name", out var nameObj) || nameObj == null)
        {
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError { Message = "Nome da ferramenta não fornecido" }
            };
        }

        var toolName = nameObj.ToString();
        var tool = McpToolRegistry.GetTool(toolName ?? string.Empty);

        if (tool == null)
        {
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError { Message = $"Ferramenta '{toolName}' não encontrada" }
            };
        }

        JsonElement? arguments = null;
        if (request.Params.TryGetValue("arguments", out var argsObj) && argsObj != null)
        {
            arguments = JsonSerializer.Deserialize<JsonElement>(argsObj.ToString() ?? "{}");
        }

        var result = await ExecuteTool(toolName, arguments ?? JsonSerializer.Deserialize<JsonElement>("{}")!);

        return new McpResponse
        {
            Id = request.Id,
            Result = new Dictionary<string, object?>
            {
                ["content"] = new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["type"] = "text",
                        ["text"] = JsonSerializer.Serialize(result.Content ?? new Dictionary<string, object?>()),
                    }
                },
                ["isError"] = !result.Success
            }
        };
    }

    private async Task<ToolResult> ExecuteTool(string toolName, JsonElement arguments)
    {
        using var scope = _serviceProvider.CreateScope();
        var agendamentoService = scope.ServiceProvider.GetRequiredService<IAgendamentoService>();
        var calendarService = scope.ServiceProvider.GetRequiredService<ICalendarService>();
        var criarValidator = scope.ServiceProvider.GetRequiredService<IValidator<CriarAgendamentoDto>>();
        var atualizarValidator = scope.ServiceProvider.GetRequiredService<IValidator<AtualizarAgendamentoDto>>();

        return toolName switch
        {
            "agendar_criar" => await AgendamentosTool.ExecuteAgendarCriar(arguments, agendamentoService, criarValidator),
            "agendar_consultar" => await AgendamentosTool.ExecuteAgendarConsultar(arguments, agendamentoService, calendarService),
            "agendar_atualizar" => await AgendamentosTool.ExecuteAgendarAtualizar(arguments, agendamentoService, atualizarValidator, calendarService),
            "agendar_deletar" => await AgendamentosTool.ExecuteAgendarDeletar(arguments, agendamentoService, calendarService),
            "agendar_disponibilidade" => await AgendamentosTool.ExecuteAgendarDisponibilidade(arguments, calendarService),
            _ => new ToolResult
            {
                Success = false,
                Errors = new[] { $"Ferramenta '{toolName}' não implementada" },
                Content = null
            }
        };
    }

    private Dictionary<string, object?> ConvertJsonSchema(JsonSchema schema)
    {
        var result = new Dictionary<string, object?>
        {
            ["type"] = schema.Type
        };

        if (!string.IsNullOrEmpty(schema.Format))
            result["format"] = schema.Format;

        if (!string.IsNullOrEmpty(schema.Description))
            result["description"] = schema.Description;

        if (schema.Properties != null)
        {
            result["properties"] = schema.Properties.ToDictionary(
                kvp => kvp.Key,
                kvp => (object?)ConvertJsonSchema(kvp.Value)
            );
        }

        if (schema.Required != null && schema.Required.Length > 0)
            result["required"] = schema.Required;

        return result;
    }

    private async Task SendErrorResponse(StreamWriter writer, string message)
    {
        var errorResponse = new McpResponse
        {
            Error = new McpError { Message = message }
        };

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await writer.WriteLineAsync(json);
    }
}

// Modelos para comunicação MCP
public class McpRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    public string? Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, object?>? Params { get; set; }
}

public class McpResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    public string? Id { get; set; }
    public Dictionary<string, object?>? Result { get; set; }
    public McpError? Error { get; set; }
}

public class McpError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
}

