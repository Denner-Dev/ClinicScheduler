namespace Backend.Domain.Entities;

public class Agendamento
{
    public int Id { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }

    public string NomePaciente { get; set; } = string.Empty;
    public string? EmailPaciente { get; set; }
}
