namespace GestionAccesos.Services;

public record CierreAutomaticoEntry(
    int IdFichaje,
    int IdTrabajador,
    string NombreTrabajador,
    string? Departamento,
    DateTime HoraEntrada,
    DateTime HoraCierre,
    double HorasTranscurridas
);

/// <summary>
/// Singleton en memoria que acumula los fichajes cerrados automáticamente
/// durante la sesión actual de la aplicación.
/// </summary>
public class FichajeAutoCloseHistorial
{
    private readonly List<CierreAutomaticoEntry> _entradas = new();
    private readonly object _lock = new();

    public IReadOnlyList<CierreAutomaticoEntry> Entradas
    {
        get { lock (_lock) { return _entradas.ToList().AsReadOnly(); } }
    }

    public void Registrar(CierreAutomaticoEntry entry)
    {
        lock (_lock) { _entradas.Add(entry); }
    }

    public void Limpiar()
    {
        lock (_lock) { _entradas.Clear(); }
    }

    /// <summary>
    /// Evita duplicados al precargar desde BD (mismo IdFichaje).
    /// </summary>
    public void RegistrarSiNoExiste(CierreAutomaticoEntry entry)
    {
        lock (_lock)
        {
            if (!_entradas.Any(e => e.IdFichaje == entry.IdFichaje))
                _entradas.Add(entry);
        }
    }
}
