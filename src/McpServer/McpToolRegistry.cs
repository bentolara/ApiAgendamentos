using Tools;

namespace McpServer;

public static class McpToolRegistry
{
    private static readonly List<ToolDefinition> _tools = new();

    public static void RegisterAllTools()
    {
        _tools.Clear();
        
        // Registrar todas as ferramentas de agendamento
        _tools.Add(AgendamentosTool.AgendarCriarTool);
        _tools.Add(AgendamentosTool.AgendarConsultarTool);
        _tools.Add(AgendamentosTool.AgendarAtualizarTool);
        _tools.Add(AgendamentosTool.AgendarDeletarTool);
        _tools.Add(AgendamentosTool.AgendarDisponibilidadeTool);
    }

    public static IReadOnlyList<ToolDefinition> GetTools() => _tools.AsReadOnly();

    public static ToolDefinition? GetTool(string name) => _tools.FirstOrDefault(t => t.Name == name);
}

