using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AziendeDati.Infrastructure.Configurations;

/// <summary>Mapping Fluent API dell'entità Ordine.</summary>
public class OrdineConfiguration : IEntityTypeConfiguration<Ordine>
{
    public void Configure(EntityTypeBuilder<Ordine> builder)
    {
        builder.ToTable("Ordini");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Numero)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.Numero).IsUnique();

        builder.HasOne(x => x.Azienda)
            .WithMany(a => a.Ordini)
            .HasForeignKey(x => x.AziendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new Ordine { Id = 1, Numero = "ORD-2025-001", Data = new DateTime(2025, 4, 1), AziendaId = 1 },
            new Ordine { Id = 2, Numero = "ORD-2025-002", Data = new DateTime(2025, 4, 20), AziendaId = 2 });
    }
}
