using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AziendeDati.Infrastructure.Configurations;

/// <summary>Mapping Fluent API dell'entità Ruolo.</summary>
public class RuoloConfiguration : IEntityTypeConfiguration<Ruolo>
{
    public void Configure(EntityTypeBuilder<Ruolo> builder)
    {
        builder.ToTable("Ruoli");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Nome).IsUnique();

        // I nomi corrispondono ai valori del claim "role" nel token JWT (Fase 8):
        // convenzione "area.risorsa.permesso", leggibile e ordinabile.
        builder.HasData(
            new Ruolo { Id = 1, Nome = "data.company.owner" },
            new Ruolo { Id = 2, Nome = "data.company.reader" });
    }
}
