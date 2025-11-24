using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agendamento> Agendamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agendamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClienteNome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ClienteTelefone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Observacoes).HasMaxLength(1000);
        });
    }
}

