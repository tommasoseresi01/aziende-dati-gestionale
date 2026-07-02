namespace AziendeDati.Api.Services;

/// <summary>
/// Servizio di esempio che fornisce la data/ora corrente.
/// </summary>
// PERCHÉ un'interfaccia per una cosa così banale? È il cuore della Dependency
// Injection: i consumatori (es. PingController) dipendono dall'ASTRAZIONE
// (IClockService), non dall'implementazione concreta (ClockService).
// Vantaggi pratici:
//   1. Testabilità: nei test si inietta un "FakeClockService" con un'ora fissa,
//      così i test non dipendono dall'orologio reale (che cambia a ogni run).
//   2. Sostituibilità: si cambia implementazione (es. ora da un time server NTP)
//      senza toccare i controller che la usano.
// ERRORE DA EVITARE: chiamare DateTime.Now direttamente nel codice applicativo.
// Funziona, ma rende il codice non testabile e sparge la dipendenza "tempo" ovunque.
public interface IClockService
{
    // DateTimeOffset invece di DateTime: porta con sé l'offset dal fuso orario
    // (es. +02:00), quindi il valore è NON ambiguo. Un DateTime "nudo" non dice
    // se è ora locale o UTC — fonte classica di bug con date salvate a DB.
    DateTimeOffset Now { get; }

    // L'ora in UTC: è la forma che si usa per SALVARE i timestamp (il fuso
    // orario è un problema di presentazione, non di persistenza).
    DateTimeOffset UtcNow { get; }
}
