namespace Backend.DTOs;

public class CriarAgendamentoDto
{
    public string NomePaciente { get; set; } = string.Empty;
    public string? EmailPaciente { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
}
