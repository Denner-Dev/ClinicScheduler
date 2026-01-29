using Backend.DTOs;
using Backend.Domain.Regras;
using Backend.Infrastructure.Integracoes.Nager;
using Backend.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Backend.Servicos;

public class ServicoDisponibilidade
{
    private readonly ClinicaDbContext _db;
    private readonly ServicoFeriados _servicoFeriados;

    public ServicoDisponibilidade(ClinicaDbContext db, ServicoFeriados servicoFeriados)
    {
        _db = db;
        _servicoFeriados = servicoFeriados;
    }

    public async Task<DisponibilidadeRespostaDto> ObterDisponibilidadeAsync(DateOnly data, CancellationToken ct)
    {
        var resp = new DisponibilidadeRespostaDto();

        if (RegrasDisponibilidade.EhFinalDeSemana(data))
        {
            resp.Motivo = "FIM_DE_SEMANA";
            return resp;
        }

        var feriado = await _servicoFeriados.ObterFeriadoAsync(data, ct);
        if (feriado is not null)
        {
            resp.Motivo = "FERIADO";
            resp.FeriadoNome = feriado.LocalName ?? feriado.Name;
            return resp;
        }

        var inicioDia = data.ToDateTime(new TimeOnly(0, 0));
        var fimDia = data.ToDateTime(new TimeOnly(23, 59, 59));

        var ocupados = await _db.Agendamentos
            .Where(a => a.Inicio >= inicioDia && a.Inicio <= fimDia)
            .Select(a => a.Inicio)
            .ToListAsync(ct);

        var ocupadosSet = new HashSet<DateTime>(ocupados);

        var horaAtual = RegrasDisponibilidade.HoraAbertura;

        while (horaAtual < RegrasDisponibilidade.HoraFechamento)
        {
            var inicio = data.ToDateTime(horaAtual);
            var fim = inicio.Add(RegrasDisponibilidade.DuracaoConsulta);

            if (TimeOnly.FromDateTime(fim) > RegrasDisponibilidade.HoraFechamento)
                break;

            if (!ocupadosSet.Contains(inicio))
                resp.Slots.Add(new HorarioDisponivelDto { Inicio = inicio, Fim = fim });

            horaAtual = horaAtual.AddHours(1);
        }

        if (resp.Slots.Count == 0)
            resp.Motivo = "OCUPADO";

        return resp;
    }
}
