namespace Application.DTOs;

public class CriarAgendamentoDto
{
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string? Observacoes { get; set; }
}

