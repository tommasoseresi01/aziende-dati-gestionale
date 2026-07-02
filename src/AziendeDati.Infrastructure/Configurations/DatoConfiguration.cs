using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AziendeDati.Infrastructure.Configurations;

/// <summary>Mapping Fluent API dell'entità Dato, con le relazioni verso Azienda e Categoria.</summary>
public class DatoConfiguration : IEntityTypeConfiguration<Dato>
{
    public void Configure(EntityTypeBuilder<Dato> builder)
    {
        builder.ToTable("Dati");
        builder.HasKey(x => x.Id);

        // HasPrecision(18, 2) → colonna decimal(18,2): 18 cifre totali, 2 decimali.
        // OBBLIGATORIO specificarla: senza, EF userebbe un default e SQL Server
        // potrebbe TRONCARE silenziosamente i decimali in più (warning a runtime).
        builder.Property(x => x.Value)
            .HasPrecision(18, 2);

        // ---------------------------------------------------------------------
        // RELAZIONE 1-N CON LA FLUENT API — si legge come una frase:
        //   "un Dato HA UNA Azienda, CON MOLTI Dati, tramite la FK AziendaId".
        // La configuriamo dal lato "N" (il figlio), UNA volta sola per relazione:
        // configurarla anche dal lato Azienda sarebbe ridondante.
        //
        // OnDelete decide cosa succede ai FIGLI quando si cancella il PADRE:
        //  - Cascade:  cancellata l'Azienda → cancellati anche i suoi Dati.
        //    Sensato: un dato senza azienda non significa nulla.
        //  - Restrict: il DB BLOCCA la cancellazione del padre se ha figli.
        //  - SetNull:  la FK dei figli diventa NULL (richiede FK nullable).
        // ---------------------------------------------------------------------
        builder.HasOne(x => x.Azienda)
            .WithMany(a => a.Dati)
            .HasForeignKey(x => x.AziendaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Verso la Categoria invece scegliamo Restrict: una categoria è una
        // tabella "di riferimento" — cancellarla NON deve trascinarsi dietro le
        // misurazioni storiche. Chi vuole eliminarla deve prima decidere cosa
        // fare dei dati che la usano (scelta esplicita, non automatica).
        builder.HasOne(x => x.Categoria)
            .WithMany(c => c.Dati)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed: PK e FK esplicite, timestamp costanti (vedi AziendaConfiguration).
        builder.HasData(
            new Dato { Id = 1, Value = 21.50m, Timestamp = new DateTime(2025, 5, 10, 8, 0, 0), AziendaId = 1, CategoriaId = 1 },
            new Dato { Id = 2, Value = 22.30m, Timestamp = new DateTime(2025, 5, 10, 14, 0, 0), AziendaId = 1, CategoriaId = 1 },
            new Dato { Id = 3, Value = 1.75m, Timestamp = new DateTime(2025, 5, 10, 8, 0, 0), AziendaId = 1, CategoriaId = 2 },
            new Dato { Id = 4, Value = 45.00m, Timestamp = new DateTime(2025, 5, 11, 8, 0, 0), AziendaId = 1, CategoriaId = 3 },
            new Dato { Id = 5, Value = 120.40m, Timestamp = new DateTime(2025, 5, 11, 8, 0, 0), AziendaId = 1, CategoriaId = 4 },
            new Dato { Id = 6, Value = 19.80m, Timestamp = new DateTime(2025, 5, 10, 8, 0, 0), AziendaId = 2, CategoriaId = 1 },
            new Dato { Id = 7, Value = 2.10m, Timestamp = new DateTime(2025, 5, 10, 9, 30, 0), AziendaId = 2, CategoriaId = 2 },
            new Dato { Id = 8, Value = 55.20m, Timestamp = new DateTime(2025, 5, 11, 10, 0, 0), AziendaId = 2, CategoriaId = 3 },
            new Dato { Id = 9, Value = 98.75m, Timestamp = new DateTime(2025, 5, 11, 10, 0, 0), AziendaId = 2, CategoriaId = 4 },
            new Dato { Id = 10, Value = 23.10m, Timestamp = new DateTime(2025, 5, 12, 8, 0, 0), AziendaId = 2, CategoriaId = 1 });
    }
}
