using AziendeDati.Application.Dtos;
using AziendeDati.Application.Validators;

namespace AziendeDati.Tests.Unit;

/// <summary>Test del validatore FluentValidation dell'Ordine (Fase 11, verifica la Fase 6).</summary>
// PERCHÉ i validator sono FACILI da testare: sono classi normali, senza dipendenze
// dal web. Si istanziano e si chiama ValidateAsync — nessun server, nessun DB.
// È il grande vantaggio di FluentValidation rispetto alle data annotations sparse.
public class OrdineCreateDtoValidatorTests
{
    private readonly OrdineCreateDtoValidator _validator = new();

    // Un ordine valido "di base" da modificare nei singoli test (helper anti-ripetizione).
    private static OrdineCreateDto OrdineValido() => new()
    {
        Numero = "ORD-2026-001",
        Data = DateTime.UtcNow,
        AziendaId = 1,
        Righe =
        [
            new RigaOrdineCreateDto { CategoriaId = 1, Descrizione = "Sensore", Quantita = 2, PrezzoUnitario = 49.90m }
        ]
    };

    [Fact]
    public async Task Un_ordine_completo_e_corretto_e_valido()
    {
        var esito = await _validator.ValidateAsync(OrdineValido());
        Assert.True(esito.IsValid);
    }

    [Fact]
    public async Task Un_ordine_senza_righe_e_invalido()
    {
        // Regola SULLA COLLEZIONE: "almeno una riga" (requisito cross-field della Fase 6).
        var dto = OrdineValido() with { Righe = [] };

        var esito = await _validator.ValidateAsync(dto);

        Assert.False(esito.IsValid);
        Assert.Contains(esito.Errors, e => e.PropertyName == "Righe");
    }

    [Fact]
    public async Task Una_riga_con_quantita_zero_e_invalida_e_l_errore_e_indicizzato()
    {
        // RuleForEach → l'errore esce indicizzato ("Righe[0].Quantita"): così il
        // client sa QUALE riga è sbagliata.
        var dto = OrdineValido() with
        {
            Righe = [new RigaOrdineCreateDto { CategoriaId = 1, Descrizione = "X", Quantita = 0, PrezzoUnitario = 1m }]
        };

        var esito = await _validator.ValidateAsync(dto);

        Assert.False(esito.IsValid);
        Assert.Contains(esito.Errors, e => e.PropertyName == "Righe[0].Quantita");
    }

    [Fact]
    public async Task Una_data_nel_futuro_e_invalida()
    {
        // Regola cross-field: la data dell'ordine non può stare nel futuro.
        var dto = OrdineValido() with { Data = DateTime.UtcNow.AddDays(5) };

        var esito = await _validator.ValidateAsync(dto);

        Assert.False(esito.IsValid);
        Assert.Contains(esito.Errors, e => e.PropertyName == nameof(OrdineCreateDto.Data));
    }
}
