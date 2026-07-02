using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AziendeDati.Infrastructure.Configurations;

/// <summary>Mapping Fluent API dell'entità Utente, con le relazioni verso Azienda e Ruolo.</summary>
public class UtenteConfiguration : IEntityTypeConfiguration<Utente>
{
    public void Configure(EntityTypeBuilder<Utente> builder)
    {
        builder.ToTable("Utenti");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .HasMaxLength(50)
            .IsRequired();

        // Username univoco: nella Fase 8 sarà la chiave di lookup della claims
        // transformation (dal claim "sub" del token → utente nel DB).
        builder.HasIndex(x => x.Username).IsUnique();

        // Il FORMATO email non si valida qui: il DB conosce solo lunghezza e
        // NOT NULL. La validazione di formato è compito dei DTO (Fase 6).
        builder.Property(x => x.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasOne(x => x.Azienda)
            .WithMany(a => a.Utenti)
            .HasForeignKey(x => x.AziendaId)
            .OnDelete(DeleteBehavior.Cascade); // via l'azienda → via i suoi utenti

        // Restrict: non si può cancellare un Ruolo mentre ci sono utenti che lo
        // usano (i ruoli sono dati di riferimento, come le Categorie per i Dati).
        builder.HasOne(x => x.Ruolo)
            .WithMany(r => r.Utenti)
            .HasForeignKey(x => x.RuoloId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Utente { Id = 1, Username = "mario.rossi", Email = "mario.rossi@acme.it", Attivo = true, AziendaId = 1, RuoloId = 1 },
            new Utente { Id = 2, Username = "laura.bianchi", Email = "laura.bianchi@acme.it", Attivo = true, AziendaId = 1, RuoloId = 2 },
            new Utente { Id = 3, Username = "giulia.verdi", Email = "giulia.verdi@globex.it", Attivo = true, AziendaId = 2, RuoloId = 1 },
            new Utente { Id = 4, Username = "paolo.neri", Email = "paolo.neri@globex.it", Attivo = true, AziendaId = 2, RuoloId = 2 });
    }
}
