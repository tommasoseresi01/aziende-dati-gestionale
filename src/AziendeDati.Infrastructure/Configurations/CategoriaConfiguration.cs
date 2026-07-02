using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AziendeDati.Infrastructure.Configurations;

/// <summary>Mapping Fluent API dell'entità Categoria.</summary>
public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorie");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .HasMaxLength(50)
            .IsRequired();

        // Nome univoco: due categorie "Temperatura" non hanno senso.
        builder.HasIndex(x => x.Nome).IsUnique();

        // Opzionale (string? nell'entità → colonna NULL-abile): qui fissiamo
        // solo la lunghezza massima. IsRequired NON va chiamato.
        builder.Property(x => x.Descrizione)
            .HasMaxLength(250);

        builder.HasData(
            new Categoria { Id = 1, Nome = "Temperatura", Descrizione = "Temperature rilevate dai sensori, in gradi Celsius" },
            new Categoria { Id = 2, Nome = "Pressione", Descrizione = "Pressioni di esercizio degli impianti, in bar" },
            new Categoria { Id = 3, Nome = "Umidità", Descrizione = "Umidità relativa ambientale, in percentuale" },
            new Categoria { Id = 4, Nome = "Consumo energetico", Descrizione = "Consumi elettrici degli impianti, in kWh" });
    }
}
