using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AziendeDati.Infrastructure.Configurations;

/// <summary>
/// Mapping Fluent API dell'entità Azienda (tabella, colonne, vincoli, seed).
/// </summary>
// FLUENT API vs DATA ANNOTATIONS: gli stessi vincoli si potrebbero esprimere con
// attributi sull'entità ([MaxLength(100)], [Required]...), ma sporcherebbero il
// dominio con dettagli di persistenza (violando la dependency rule) e non
// coprono tutto (es. HasData, indici con filtro). La Fluent API tiene TUTTO il
// mapping in Infrastructure ed è più potente: è la scelta enterprise standard.
public class AziendaConfiguration : IEntityTypeConfiguration<Azienda>
{
    public void Configure(EntityTypeBuilder<Azienda> builder)
    {
        // Nome tabella esplicito (senza, EF userebbe il nome del DbSet: uguale,
        // ma scriverlo rende il mapping evidente — niente magia).
        builder.ToTable("Aziende");

        // Chiave primaria esplicita. EF la troverebbe per convenzione ("Id"),
        // ma dichiararla documenta l'intenzione.
        builder.HasKey(x => x.Id);

        // Mapping colonna con nome PERSONALIZZATO: nel DB la colonna si chiama
        // RAG_SOC (stile "legacy" tipico dei gestionali reali), ma nel codice
        // usiamo il nome leggibile RagioneSociale. HasColumnName disaccoppia i
        // due mondi: il DB può avere le sue convenzioni, il C# le sue.
        builder.Property(x => x.RagioneSociale)
            .HasColumnName("RAG_SOC")
            .HasMaxLength(100)   // → nvarchar(100) invece di nvarchar(max): mai colonne illimitate senza motivo
            .IsRequired();       // → NOT NULL (ridondante col "required" C#, ma esplicito nel mapping)

        builder.Property(x => x.PartitaIva)
            .HasColumnName("P_IVA")
            .HasMaxLength(11)
            .IsRequired();

        // INDICE UNIVOCO: il vincolo "due aziende non possono avere la stessa
        // P.IVA" deve vivere NEL DATABASE, non solo nel codice: è l'unica
        // garanzia vera contro i duplicati (due richieste concorrenti
        // supererebbero entrambe un controllo fatto solo in C#).
        builder.HasIndex(x => x.PartitaIva).IsUnique();

        // SEED con HasData: i dati diventano parte del MODELLO e finiscono
        // DENTRO la migration (INSERT generati). Regole di HasData:
        //  - PK esplicite obbligatorie (niente identity auto-generata);
        //  - valori COSTANTI (niente DateTime.Now: la migration deve essere
        //    deterministica, uguale a ogni generazione).
        // Alternativa citata: un "seeder" eseguito all'avvio dell'app, meglio
        // per dati grossi o dinamici. Per dati di riferimento piccoli e stabili
        // HasData è perfetto perché versionato insieme allo schema.
        // Fonte: https://learn.microsoft.com/ef/core/modeling/data-seeding
        builder.HasData(
            new Azienda { Id = 1, RagioneSociale = "ACME S.p.A.", PartitaIva = "01234567890", DataRegistrazione = new DateTime(2024, 3, 1) },
            new Azienda { Id = 2, RagioneSociale = "Globex S.r.l.", PartitaIva = "09876543210", DataRegistrazione = new DateTime(2024, 6, 15) });
    }
}
