namespace AziendeDati.Domain.ReadModels;

/// <summary>
/// READ MODEL: risultato della query di report "somma dei valori per categoria".
/// </summary>
// Non è un'entità (non esiste una tabella "SommaPerCategoria"): è la FORMA del
// risultato di un'aggregazione. Tenerlo nel dominio permette al repository di
// restituirlo senza conoscere i DTO dell'API (che vivono in Application).
// Proprietà con "init" (e non costruttore posizionale): EF Core materializza i
// risultati delle proiezioni valorizzando le proprietà una a una.
public sealed record SommaPerCategoria
{
    public int CategoriaId { get; init; }
    public required string Categoria { get; init; }
    public decimal Somma { get; init; }
}
