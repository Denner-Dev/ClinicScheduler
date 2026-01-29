using System.Net.Http.Json;

namespace Backend.Infrastructure.Integracoes.Nager;

public class ClienteNager
{
    private readonly HttpClient _http;

    public ClienteNager(HttpClient http) => _http = http;

    public async Task<List<NagerFeriadoDto>> ObterFeriadosPublicosAsync(int ano, string countryCode, CancellationToken ct)
    {
        var path = $"/api/v3/PublicHolidays/{ano}/{countryCode}";

        using var resp = await _http.GetAsync(path, ct);

        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Não foi possível consultar feriados no momento. Tente novamente.");
        }

        var feriados = await resp.Content.ReadFromJsonAsync<List<NagerFeriadoDto>>(cancellationToken: ct);
        return feriados ?? [];
    }
}
