using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.Helpers.Crypton;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Services;

/// <summary>
/// Servicio en background que cierra automáticamente las visitas que llevan
/// más de 8 horas abiertas sin registrar salida. Se ejecuta cada 10 minutos.
/// </summary>
public class VisitaAutoCloseService : BackgroundService
{
    private const int UmbralHoras = 8;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VisitaAutoCloseService> _logger;

    public VisitaAutoCloseService(
        IServiceScopeFactory scopeFactory,
        ILogger<VisitaAutoCloseService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VisitaAutoCloseService iniciado. Umbral: {Umbral}h. Intervalo: {Intervalo}min.",
            UmbralHoras, Intervalo.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CerrarVisitasAntiguas(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en VisitaAutoCloseService.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task CerrarVisitasAntiguas(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var crypto = scope.ServiceProvider.GetRequiredService<ICryptoService>();

        var umbral = DateTime.Now.AddHours(-UmbralHoras);

        var visitasAbiertas = await context.Visitas
            .Where(v => (v.Borrado == null || v.Borrado == false)
                     && v.FechaSalida == null
                     && v.FechaEntrada != null
                     && v.FechaEntrada <= umbral)
            .Include(v => v.IdVisitanteNavigation)
            .ToListAsync(ct);

        if (visitasAbiertas.Count == 0)
            return;

        foreach (var visita in visitasAbiertas)
        {
            var horaCierre = visita.FechaEntrada!.Value.AddHours(UmbralHoras);
            visita.FechaSalida = horaCierre;

            var nombre = ObtenerNombreVisitante(visita, crypto);

            _logger.LogWarning(
                "Cierre automático de visita #{Id} (visitante: {Nombre}). Entrada: {Entrada}. Cierre fijado a: {Cierre} ({Umbral}h).",
                visita.IdVisita, nombre, visita.FechaEntrada, horaCierre, UmbralHoras);
        }

        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Se cerraron {Count} visita(s) automáticamente por superar {Umbral}h.",
            visitasAbiertas.Count, UmbralHoras);

        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var ahora = DateTime.Now;
            var filas = string.Join("", visitasAbiertas.Select(v =>
            {
                var nombre = ObtenerNombreVisitante(v, crypto);
                return $"<tr><td>#{v.IdVisita}</td><td>{nombre}</td><td>{v.FechaEntrada:dd/MM/yyyy HH:mm}</td><td>{v.FechaSalida:HH:mm}</td></tr>";
            }));

            var html = $"""
                <h2 style="color:#a91c32">⚠️ Cierre automático de visitas</h2>
                <p>El sistema ha cerrado <strong>{visitasAbiertas.Count}</strong> visita(s) por superar <strong>{UmbralHoras} horas</strong> sin registrar salida.</p>
                <table border="1" cellpadding="6" cellspacing="0" style="border-collapse:collapse;font-family:sans-serif">
                    <thead style="background:#f1f5f9">
                        <tr><th>ID Visita</th><th>Visitante</th><th>Entrada</th><th>Salida auto.</th></tr>
                    </thead>
                    <tbody>{filas}</tbody>
                </table>
                <p style="color:#64748b;font-size:.85em">Generado automáticamente por GestionAccesos · {ahora:dd/MM/yyyy HH:mm}</p>
                """;

            await emailService.SendEmailToAdminAsync("⚠️ Cierre automático de visitas — GestionAccesos", html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación de cierre automático de visitas.");
        }
    }

    private static string ObtenerNombreVisitante(Entities.Visita visita, ICryptoService crypto)
    {
        if (visita.IdVisitanteNavigation is null)
            return $"Visitante #{visita.IdVisitante}";

        var nombre = TryDecrypt(visita.IdVisitanteNavigation.Nombre, crypto);
        var ap1 = TryDecrypt(visita.IdVisitanteNavigation.PrimerApellido, crypto);
        return string.Join(" ", new[] { nombre, ap1 }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static string? TryDecrypt(string? valor, ICryptoService crypto)
    {
        if (string.IsNullOrWhiteSpace(valor)) return valor;
        try { return crypto.Decrypt(valor); }
        catch { return valor; }
    }
}
