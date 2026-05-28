using GestionAccesos.Data.GestionAccesos;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Services;

/// <summary>
/// Servicio en background que cierra automáticamente los fichajes que superan
/// el umbral de horas configurable desde Parámetros Worker. Se ejecuta cada 5 minutos.
/// </summary>
public class FichajeAutoCloseService : BackgroundService
{
    private const int UmbralHorasPorDefecto = 9;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FichajeAutoCloseService> _logger;
    private readonly FichajeAutoCloseHistorial _historial;

    public FichajeAutoCloseService(
        IServiceScopeFactory scopeFactory,
        ILogger<FichajeAutoCloseService> logger,
        FichajeAutoCloseHistorial historial)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _historial = historial;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FichajeAutoCloseService iniciado. Umbral por defecto: {Umbral}h. Intervalo: {Intervalo}min.",
            UmbralHorasPorDefecto, Intervalo.TotalMinutes);

        await PrecargarHistorialAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CerrarFichajesAnomulos(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en FichajeAutoCloseService.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task<int> LeerUmbralAsync(AppDbContext context, CancellationToken ct)
    {
        var param = await context.ParametrosWorkers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Tipo == "horas_cierre_fichaje" && p.Activo == 1 && p.Borrado == 0, ct);

        if (param != null && int.TryParse(param.Valor, out var val) && val > 0)
            return val;

        return UmbralHorasPorDefecto;
    }

    private async Task CerrarFichajesAnomulos(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var crypto = scope.ServiceProvider.GetRequiredService<ICryptoService>();

        var umbralHoras = await LeerUmbralAsync(context, ct);
        var umbral = DateTime.Now.AddHours(-umbralHoras);

        var fichajesAbiertos = await context.FichajesTrabajadors
            .Where(f => (f.Borrado == null || f.Borrado == false)
                     && f.HoraSalida == null
                     && f.HoraEntrada != null
                     && f.HoraEntrada <= umbral)
            .Include(f => f.IdTrabajadorNavigation)
            .ToListAsync(ct);

        if (fichajesAbiertos.Count == 0)
            return;

        foreach (var fichaje in fichajesAbiertos)
        {
            var horaCierre = fichaje.HoraEntrada!.Value.AddHours(umbralHoras);
            fichaje.HoraSalida = horaCierre;

            var trabajador = fichaje.IdTrabajadorNavigation;
            var nombre = trabajador is null ? $"Trabajador #{fichaje.IdTrabajador}" : DescifrarNombre(trabajador, crypto);
            var departamento = trabajador?.Departamento is null ? null : TryDecrypt(trabajador.Departamento, crypto);

            _historial.Registrar(new CierreAutomaticoEntry(
                IdFichaje: fichaje.IdFichaje,
                IdTrabajador: fichaje.IdTrabajador ?? 0,
                NombreTrabajador: nombre,
                Departamento: departamento,
                HoraEntrada: fichaje.HoraEntrada.Value,
                HoraCierre: horaCierre,
                HorasTranscurridas: umbralHoras
            ));

            _logger.LogWarning(
                "Cierre automático del fichaje #{Id} (trabajador #{Trabajador}). " +
                "Entrada: {Entrada}. Cierre fijado a: {Cierre} ({Umbral}h).",
                fichaje.IdFichaje,
                fichaje.IdTrabajador,
                fichaje.HoraEntrada,
                horaCierre,
                umbralHoras);
        }

        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Se cerraron {Count} fichaje(s) automáticamente por superar {Umbral}h.",
            fichajesAbiertos.Count, umbralHoras);

        // Notificar al administrador por email
        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var ahora = DateTime.Now;
            var filas = string.Join("", fichajesAbiertos.Select(f =>
                $"<tr><td>#{f.IdFichaje}</td><td>Trabajador #{f.IdTrabajador}</td><td>{f.HoraEntrada:dd/MM/yyyy HH:mm}</td><td>{f.HoraSalida:HH:mm}</td></tr>"));

            var html = $"""
                <h2 style="color:#a91c32">⚠️ Cierre automático de fichajes</h2>
                <p>El sistema ha cerrado <strong>{fichajesAbiertos.Count}</strong> fichaje(s) por superar <strong>{umbralHoras} horas</strong> de jornada.</p>
                <table border="1" cellpadding="6" cellspacing="0" style="border-collapse:collapse;font-family:sans-serif">
                    <thead style="background:#f1f5f9">
                        <tr><th>ID Fichaje</th><th>Trabajador</th><th>Entrada</th><th>Salida auto.</th></tr>
                    </thead>
                    <tbody>{filas}</tbody>
                </table>
                <p style="color:#64748b;font-size:.85em">Generado automáticamente por GestionAccesos · {ahora:dd/MM/yyyy HH:mm}</p>
                """;

            await emailService.SendEmailToAdminAsync("⚠️ Cierre automático de fichajes — GestionAccesos", html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación de cierre automático.");
        }
    }

    /// <summary>
    /// Al arrancar, carga del historial de BD los fichajes que fueron cerrados
    /// automáticamente (detectados porque HoraSalida == HoraEntrada + 9h exactamente).
    /// </summary>
    private async Task PrecargarHistorialAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var crypto = scope.ServiceProvider.GetRequiredService<ICryptoService>();

            var desde = DateTime.Now.AddDays(-30);

            var candidatos = await context.FichajesTrabajadors
                .Where(f => (f.Borrado == null || f.Borrado == false)
                         && f.HoraSalida != null
                         && f.HoraEntrada != null
                         && f.HoraEntrada >= desde)
                .Include(f => f.IdTrabajadorNavigation)
                .ToListAsync(ct);

            var umbralHoras = await LeerUmbralAsync(context, ct);

            int cargados = 0;
            foreach (var f in candidatos)
            {
                var duracion = (f.HoraSalida!.Value - f.HoraEntrada!.Value).TotalHours;
                // Detectamos cierres automáticos: duración exactamente igual al umbral configurado (±1 min)
                if (Math.Abs(duracion - umbralHoras) > (1.0 / 60)) continue;

                var trabajador = f.IdTrabajadorNavigation;
                var nombre = trabajador is null ? $"Trabajador #{f.IdTrabajador}" : DescifrarNombre(trabajador, crypto);
                var departamento = trabajador?.Departamento is null ? null : TryDecrypt(trabajador.Departamento, crypto);

                _historial.RegistrarSiNoExiste(new CierreAutomaticoEntry(
                    IdFichaje: f.IdFichaje,
                    IdTrabajador: f.IdTrabajador ?? 0,
                    NombreTrabajador: nombre,
                    Departamento: departamento,
                    HoraEntrada: f.HoraEntrada.Value,
                    HoraCierre: f.HoraSalida.Value,
                    HorasTranscurridas: umbralHoras
                ));
                cargados++;
            }

            if (cargados > 0)
                _logger.LogInformation("Historial precargado: {Count} cierre(s) automático(s) de sesiones anteriores.", cargados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al precargar historial de cierres automáticos.");
        }
    }

    private static string DescifrarNombre(Entities.Trabajadore t, ICryptoService crypto)
    {
        var nombre = TryDecrypt(t.Nombre, crypto);
        var ap1 = TryDecrypt(t.Apellido1, crypto);
        var ap2 = TryDecrypt(t.Apellido2, crypto);
        return string.Join(" ", new[] { nombre, ap1, ap2 }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static string? TryDecrypt(string? valor, ICryptoService crypto)
    {
        if (string.IsNullOrWhiteSpace(valor)) return valor;
        try { return crypto.Decrypt(valor); }
        catch { return valor; }
    }
}
