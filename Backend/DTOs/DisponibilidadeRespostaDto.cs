namespace Backend.DTOs;

public class DisponibilidadeRespostaDto
{
    public List<HorarioDisponivelDto> Slots { get; set; } = [];
    public string? Motivo { get; set; }
    public string? FeriadoNome { get; set; }
}
