using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistencia;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options) { }

    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agendamento>(e =>
        {
            e.ToTable("Agendamentos");
            e.HasKey(x => x.Id);

            // IDENTITY
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.NomePaciente)
                .HasMaxLength(150)
                .IsRequired();

            e.Property(x => x.EmailPaciente)
                .HasMaxLength(200);

            // Evita dois agendamentos com o mesmo início/fim
            e.HasIndex(x => new { x.Inicio, x.Fim }).IsUnique();
        });
    }
}
