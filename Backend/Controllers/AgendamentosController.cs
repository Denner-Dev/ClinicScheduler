using Backend.DTOs;
using Backend.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
public class AgendamentosController : ControllerBase
{
    private readonly ServicoAgendamentos _servico;

    public AgendamentosController(ServicoAgendamentos servico)
    {
        _servico = servico;
    }

    [HttpGet("/appointments")]
    public async Task<ActionResult<List<AgendamentoDto>>> Get(CancellationToken ct)
    {
        var lista = await _servico.ListarAsync(ct);
        return Ok(lista);
    }

    [HttpPost("/appointments")]
    public async Task<ActionResult<AgendamentoDto>> Post([FromBody] CriarAgendamentoDto dto, CancellationToken ct)
    {
        try
        {
            var criado = await _servico.CriarAsync(dto, ct);
            return Ok(criado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
