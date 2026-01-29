using Backend.DTOs;
using Backend.Domain.Entities;
using Backend.Domain.Regras;
using Backend.Infrastructure.Integracoes.Nager;
using Backend.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Backend.Servicos;

public class ServicoAgendamentos
{
    private readonly ClinicaDbContext _db;
    private readonly ServicoFeriados _servicoFeriados;

    private static readonly DateOnly MinData = new(2026, 1, 1);
    private static readonly DateOnly MaxData = new(2035, 12, 31);

    public ServicoAgendamentos(ClinicaDbContext db, ServicoFeriados servicoFeriados)
    {
        _db = db;
        _servicoFeriados = servicoFeriados;
    }

    public async Task<AgendamentoDto> CriarAsync(CriarAgendamentoDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.NomePaciente))
            throw new InvalidOperationException("Informe o nome do paciente.");

        if (!RegrasDisponibilidade.ConsultaTemDuracaoCorreta(dto.Inicio, dto.Fim))
            throw new InvalidOperationException("A consulta deve ter duração de 1 hora.");

        if (!RegrasDisponibilidade.DentroDaJanela(dto.Inicio, dto.Fim))
            throw new InvalidOperationException("Agendamento fora do horário de funcionamento (08:00–18:00).");

        var data = DateOnly.FromDateTime(dto.Inicio);

        if (data < MinData || data > MaxData)
            throw new InvalidOperationException("Data fora do intervalo permitido (01/01/2026 a 31/12/2035).");

        if (RegrasDisponibilidade.EhFinalDeSemana(data))
            throw new InvalidOperationException("Não é permitido agendar em final de semana.");

        if (await _servicoFeriados.EhFeriadoAsync(data, ct))
            throw new InvalidOperationException("Não é permitido agendar em feriado.");

        var existe = await _db.Agendamentos.AnyAsync(a => a.Inicio == dto.Inicio, ct);
        if (existe)
            throw new InvalidOperationException("Horário já ocupado.");

        var entidade = new Agendamento
        {
            NomePaciente = dto.NomePaciente.Trim(),
            EmailPaciente = string.IsNullOrWhiteSpace(dto.EmailPaciente) ? null : dto.EmailPaciente.Trim(),
            Inicio = dto.Inicio,
            Fim = dto.Fim
        };

        _db.Agendamentos.Add(entidade);
        await _db.SaveChangesAsync(ct);

        return new AgendamentoDto
        {
            Id = entidade.Id,
            NomePaciente = entidade.NomePaciente,
            EmailPaciente = entidade.EmailPaciente,
            Inicio = entidade.Inicio,
            Fim = entidade.Fim
        };
    }

    public async Task<List<AgendamentoDto>> ListarAsync(CancellationToken ct)
    {
        return await _db.Agendamentos
            .OrderBy(a => a.Inicio)
            .Select(a => new AgendamentoDto
            {
                Id = a.Id,
                NomePaciente = a.NomePaciente,
                EmailPaciente = a.EmailPaciente,
                Inicio = a.Inicio,
                Fim = a.Fim
            })
            .ToListAsync(ct);
    }
}
