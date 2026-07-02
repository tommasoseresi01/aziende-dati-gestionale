using AziendeDati.Domain.Entities;

namespace AziendeDati.Domain.Repositories;

/// <summary>Contratto di accesso ai dati per le Aziende.</summary>
// PERCHÉ IL REPOSITORY (e perché l'INTERFACCIA sta QUI in Domain):
// i servizi di Application hanno bisogno del database, ma Application NON può
// riferire Infrastructure (dependency rule, vedi ARCHITETTURA.md). Soluzione:
// il dominio dichiara il CONTRATTO (questa interfaccia — è una "interfaccia di
// dominio": parla solo di entità), Infrastructure lo IMPLEMENTA col DbContext,
// e in Program.cs la DI collega i due. Le dipendenze a compile-time puntano
// verso il centro (l'astrazione), l'implementazione arriva a runtime:
// si chiama Dependency Inversion (la "D" di SOLID).
//
// NOTA ONESTA: il DbContext di EF È GIÀ repository + unit of work (lo dice la
// doc Microsoft stessa). Il layer in più si giustifica qui per: (1) rispettare
// la dependency rule, (2) testare i servizi senza database sostituendo questa
// interfaccia con un finto. In progetti piccoli usare il DbContext direttamente
// nei servizi è una scelta legittima.
//
// Tutti i metodi sono ASYNC e accettano un CancellationToken: se il client
// abbandona la richiesta HTTP, la query può essere annullata invece di
// continuare a consumare risorse (il token arriva dal controller).
public interface IAziendeRepository
{
    /// <summary>Tutte le aziende (sola lettura, senza tracking).</summary>
    Task<List<Azienda>> GetAllAsync(CancellationToken ct = default);

    /// <summary>True se esiste un'azienda con questo Id (query leggera, niente caricamento entità).</summary>
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    /// <summary>Azienda per Id, TRACCIATA (pronta per essere modificata), o null.</summary>
    Task<Azienda?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Inserisce e salva; al ritorno l'entità ha l'Id generato dal DB.</summary>
    Task AddAsync(Azienda azienda, CancellationToken ct = default);

    /// <summary>Salva le modifiche fatte a un'entità tracciata.</summary>
    Task UpdateAsync(Azienda azienda, CancellationToken ct = default);

    /// <summary>Elimina e salva.</summary>
    Task DeleteAsync(Azienda azienda, CancellationToken ct = default);
}
