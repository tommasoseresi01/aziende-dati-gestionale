namespace AziendeDati.Application.Dtos;

// DTO per Ordine — il "DTO complesso" della Fase 6: ha una COLLEZIONE di righe
// e regole che coinvolgono più campi insieme. Qui NIENTE data annotations:
// la validazione la fa FluentValidation (OrdineCreateDtoValidator), che gestisce
// molto meglio regole cross-field e su collezioni. Confronto nei commenti del validator.

/// <summary>Riga di un ordine in creazione (input).</summary>
public sealed record RigaOrdineCreateDto
{
    public int CategoriaId { get; init; }
    public string Descrizione { get; init; } = string.Empty;
    public int Quantita { get; init; }
    public decimal PrezzoUnitario { get; init; }
}

/// <summary>Ordine in creazione, con le sue righe (input).</summary>
public sealed record OrdineCreateDto
{
    public string Numero { get; init; } = string.Empty;
    public DateTime Data { get; init; }
    public int AziendaId { get; init; }

    // La collezione annidata: il client invia l'ordine COMPLETO di righe in un
    // unico POST (creazione atomica: o tutto o niente, vedi il servizio).
    public List<RigaOrdineCreateDto> Righe { get; init; } = [];
}

/// <summary>Riga di un ordine restituita dall'API (output).</summary>
public sealed record RigaOrdineReadDto
{
    public int Id { get; init; }
    public int CategoriaId { get; init; }
    public required string CategoriaNome { get; init; }
    public required string Descrizione { get; init; }
    public int Quantita { get; init; }
    public decimal PrezzoUnitario { get; init; }

    // Campo CALCOLATO, non esiste nel DB: il DTO serve il client, e al client
    // il totale riga serve spesso — meglio calcolarlo una volta qui che in
    // ogni frontend.
    public decimal Totale { get; init; }
}

/// <summary>Ordine restituito dall'API, completo di righe (output).</summary>
public sealed record OrdineReadDto
{
    public int Id { get; init; }
    public required string Numero { get; init; }
    public DateTime Data { get; init; }
    public int AziendaId { get; init; }
    public List<RigaOrdineReadDto> Righe { get; init; } = [];
    public decimal Totale { get; init; }
}
