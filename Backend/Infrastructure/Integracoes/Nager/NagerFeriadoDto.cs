namespace Backend.Infrastructure.Integracoes.Nager;

public class NagerFeriadoDto
{
    public DateOnly Date { get; set; }
    public string? LocalName { get; set; }
    public string? Name { get; set; }
    public string? CountryCode { get; set; }
    public bool Fixed { get; set; }
    public bool Global { get; set; }
}
