using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Ausencia.Command;

public class GestionarAusenciaCommand : IRequest<SingleResult<string>>
{
    public GestionarAusenciaCommand(
        int trabajadorId,
        string accion,
        int? tipoAusenciaId = null,
        string? observaciones = null)
    {
        TrabajadorId = trabajadorId;
        Accion = accion.ToLower();
        TipoAusenciaId = tipoAusenciaId;
        Observaciones = observaciones;
    }

    public int TrabajadorId { get; }
    public string Accion { get; }
    public int? TipoAusenciaId { get; }
    public string? Observaciones { get; }
}

internal class GestionarAusenciaCommandHandler
    : IRequestHandler<GestionarAusenciaCommand, SingleResult<string>>
{
    private readonly AppDbContext _context;

    public GestionarAusenciaCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<string>> Handle(
        GestionarAusenciaCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<string>
        {
            Data = string.Empty,
            Errors = new List<string>()
        };

        try
        {
            switch (request.Accion)
            {
                case "abrir":
                    await CerrarFichajeSiExiste(request.TrabajadorId, cancellationToken);

                    await AbrirAusencia(
                        request.TrabajadorId,
                        request.TipoAusenciaId ?? 0,
                        request.Observaciones ?? string.Empty,
                        cancellationToken);

                    result.Data = "Ausencia registrada correctamente.";
                    break;

                case "cerrar":
                    var cerrada = await CerrarAusencia(request.TrabajadorId, cancellationToken);

                    result.Data = cerrada
                        ? "Ausencia cerrada correctamente."
                        : "No hay ausencia abierta para cerrar.";

                    break;

                default:
                    result.Errors.Add("Acción no válida. Usa 'abrir' o 'cerrar'.");
                    break;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error al gestionar la ausencia: {ex.InnerException?.Message ?? ex.Message}");
        }

        return result;
    }

    private async Task CerrarFichajeSiExiste(
        int trabajadorId,
        CancellationToken cancellationToken)
    {
        var fichaje = await _context.FichajesTrabajadors
            .FirstOrDefaultAsync(f =>
                f.IdTrabajador == trabajadorId &&
                f.HoraSalida == null &&
                (f.Borrado == null || f.Borrado == false),
                cancellationToken);

        if (fichaje == null)
            return;

        fichaje.HoraSalida = DateTime.Now;

        _context.FichajesTrabajadors.Update(fichaje);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task AbrirAusencia(
        int trabajadorId,
        int motivoId,
        string observaciones,
        CancellationToken cancellationToken)
    {
        var ausencia = new AusenciasTrabajador
        {
            IdTrabajador = trabajadorId,
            Motivo = motivoId,
            Observaciones = observaciones,
            HoraInicio = DateTime.Now,
            HoraFin = null,
            FechaRegistro = DateTime.Now,
            Borrado = false
        };

        await _context.AusenciasTrabajadors.AddAsync(ausencia, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> CerrarAusencia(
        int trabajadorId,
        CancellationToken cancellationToken)
    {
        var ausencia = await _context.AusenciasTrabajadors
            .OrderByDescending(a => a.HoraInicio)
            .FirstOrDefaultAsync(a =>
                a.IdTrabajador == trabajadorId &&
                a.HoraFin == null &&
                (a.Borrado == null || a.Borrado == false),
                cancellationToken);

        if (ausencia == null)
            return false;

        ausencia.HoraFin = DateTime.Now;

        _context.AusenciasTrabajadors.Update(ausencia);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}