using AziendeDati.Application.Dtos;
using AziendeDati.Domain.Entities;

namespace AziendeDati.Application.Mappings;

/// <summary>Mapping manuale Azienda ↔ DTO.</summary>
// MAPPING A MANO, non AutoMapper. PERCHÉ: il mapping esplicito si legge, si
// debugga col breakpoint e il compilatore segnala subito i campi mancanti.
// ALTERNATIVA citata: AutoMapper (o Mapster) mappa per convenzione con meno
// codice, ma nasconde COSA viene copiato: un campo rinominato smette
// silenziosamente di essere mappato. Con 3 campi il gioco non vale la candela;
// diventa interessante con decine di DTO — sapendo cosa fa sotto.
//
// "Extension methods" (this Azienda...): si usano come se fossero metodi
// dell'entità (azienda.ToReadDto()) ma vivono qui, in Application — il dominio
// resta pulito, senza sapere nulla dei DTO.
public static class AziendaMappings
{
    /// <summary>Entità → DTO di lettura (per le risposte dell'API).</summary>
    public static AziendaReadDto ToReadDto(this Azienda entity) => new()
    {
        Id = entity.Id,
        RagioneSociale = entity.RagioneSociale,
        PartitaIva = entity.PartitaIva,
        DataRegistrazione = entity.DataRegistrazione
    };

    /// <summary>DTO di creazione → nuova entità (Id e DataRegistrazione li mette il server).</summary>
    public static Azienda ToEntity(this AziendaCreateDto dto) => new()
    {
        RagioneSociale = dto.RagioneSociale,
        PartitaIva = dto.PartitaIva
    };

    /// <summary>Applica il DTO di aggiornamento a un'entità ESISTENTE (già tracciata da EF).</summary>
    // Non si crea un'entità nuova: si modificano le proprietà di quella caricata
    // dal DB, così il change tracker di EF rileva il diff e genera l'UPDATE.
    public static void ApplyTo(this AziendaUpdateDto dto, Azienda entity)
    {
        entity.RagioneSociale = dto.RagioneSociale;
        entity.PartitaIva = dto.PartitaIva;
    }
}
