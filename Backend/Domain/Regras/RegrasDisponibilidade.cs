namespace Backend.Domain.Regras;

public static class RegrasDisponibilidade
{
    public static readonly TimeOnly HoraAbertura = new(8, 0);
    public static readonly TimeOnly HoraFechamento = new(18, 0);
    public static readonly TimeSpan DuracaoConsulta = TimeSpan.FromHours(1);

    public static bool EhFinalDeSemana(DateOnly data)
        => data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool ConsultaTemDuracaoCorreta(DateTime inicio, DateTime fim)
        => (fim - inicio) == DuracaoConsulta;

    public static bool DentroDaJanela(DateTime inicio, DateTime fim)
    {
        var data = DateOnly.FromDateTime(inicio);
        var inicioT = TimeOnly.FromDateTime(inicio);
        var fimT = TimeOnly.FromDateTime(fim);

        return DateOnly.FromDateTime(fim) == data
               && inicioT >= HoraAbertura
               && fimT <= HoraFechamento;
    }
}