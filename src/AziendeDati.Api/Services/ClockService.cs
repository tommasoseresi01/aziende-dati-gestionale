namespace AziendeDati.Api.Services;

/// <summary>
/// Implementazione reale di <see cref="IClockService"/>: legge l'orologio di sistema.
/// </summary>
// La classe è "sealed": nessuno deve ereditarla. Dichiararlo è una micro best
// practice (intenzione esplicita + piccole ottimizzazioni del runtime).
//
// NOTA: questa classe è STATELESS (nessun campo, nessuno stato condiviso), quindi
// è intrinsecamente thread-safe e può essere registrata come Singleton senza rischi
// (vedi il commento sui lifetime in Program.cs).
//
// ALTERNATIVA MODERNA: da .NET 8 esiste l'astrazione ufficiale System.TimeProvider
// (TimeProvider.System, con FakeTimeProvider per i test). Qui scriviamo la nostra
// interfaccia per imparare il meccanismo della DI; in un progetto reale valuta
// TimeProvider. Fonte: https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview
public sealed class ClockService : IClockService
{
    public DateTimeOffset Now => DateTimeOffset.Now;

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
