using Backend.DTOs;
using Backend.Servicos;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Backend.Controllers;

[ApiController]
public class DisponibilidadeController : ControllerBase
{
    private readonly ServicoDisponibilidade _servico;

    public DisponibilidadeController(ServicoDisponibilidade servico)
    {
        _servico = servico;
    }

    [HttpGet("/available")]
    public async Task<ActionResult<DisponibilidadeRespostaDto>> Get([FromQuery] string date, CancellationToken ct)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            return BadRequest("Parâmetro 'date' inválido. Use YYYY-MM-DD.");

        var min = new DateOnly(2026, 1, 1);
        var max = new DateOnly(2035, 12, 31);

        if (data < min || data > max)
            return BadRequest("Data fora do intervalo permitido (01/01/2026 a 31/12/2035).");

        try
        {
            var resp = await _servico.ObterDisponibilidadeAsync(data, ct);
            return Ok(resp);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
