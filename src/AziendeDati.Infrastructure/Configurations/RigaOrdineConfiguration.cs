using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AziendeDati.Infrastructure.Configurations;

/// <summary>Mapping Fluent API dell'entità RigaOrdine.</summary>
public class RigaOrdineConfiguration : IEntityTypeConfiguration<RigaOrdine>
{
    public void Configure(EntityTypeBuilder<RigaOrdine> builder)
    {
        builder.ToTable("RigheOrdine");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descrizione)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PrezzoUnitario)
            .HasPrecision(18, 2);

        // Cascade padre→figlio: le righe appartengono all'ordine, senza di esso
        // non hanno significato (composizione, non semplice associazione).
        builder.HasOne(x => x.Ordine)
            .WithMany(o => o.Righe)
            .HasForeignKey(x => x.OrdineId)
            .OnDelete(DeleteBehavior.Cascade);

        // La Categoria è dato di riferimento → Restrict (vedi DatoConfiguration).
        // NOTA: qui la Categoria non ha una navigation verso le righe
        // (WithMany() senza argomento = relazione "unidirezionale"): dalle righe
        // si risale alla categoria, ma non serve il contrario.
        builder.HasOne(x => x.Categoria)
            .WithMany()
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new RigaOrdine { Id = 1, OrdineId = 1, CategoriaId = 1, Descrizione = "Sensore temperatura TX-100", Quantita = 5, PrezzoUnitario = 79.90m },
            new RigaOrdine { Id = 2, OrdineId = 1, CategoriaId = 2, Descrizione = "Sensore pressione PX-20", Quantita = 2, PrezzoUnitario = 149.50m },
            new RigaOrdine { Id = 3, OrdineId = 2, CategoriaId = 3, Descrizione = "Igrometro ambientale HX-5", Quantita = 3, PrezzoUnitario = 59.00m },
            new RigaOrdine { Id = 4, OrdineId = 2, CategoriaId = 4, Descrizione = "Contatore energia EX-1", Quantita = 1, PrezzoUnitario = 320.00m });
    }
}
