namespace Backend.Infrastructure.Integracoes.Nager;

public class ServicoFeriados
{
    private readonly ClienteNager _clienteNager;
    private const string CountryCode = "BR";

    public ServicoFeriados(ClienteNager clienteNager)
    {
        _clienteNager = clienteNager;
    }

    public async Task<NagerFeriadoDto?> ObterFeriadoAsync(DateOnly data, CancellationToken ct)
    {
        var feriadosAno = await _clienteNager.ObterFeriadosPublicosAsync(data.Year, CountryCode, ct);
        return feriadosAno.FirstOrDefault(f => f.Date == data);
    }

    public async Task<bool> EhFeriadoAsync(DateOnly data, CancellationToken ct)
        => (await ObterFeriadoAsync(data, ct)) is not null;
}
