using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Fichaje.Command;

public class GestionarPausaFichajeCommand : IRequest<SingleResult<string>>
{
    public GestionarPausaFichajeCommand(int trabajadorId, string accion)
    {
        TrabajadorId = trabajadorId;
        Accion = accion.ToLower();
    }

    public int TrabajadorId { get; }
    public string Accion { get; }
}

internal class GestionarPausaFichajeCommandHandler
    : IRequestHandler<GestionarPausaFichajeCommand, SingleResult<string>>
{
    private readonly AppDbContext _context;

    public GestionarPausaFichajeCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<string>> Handle(
        GestionarPausaFichajeCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<string>
        {
            Data = string.Empty,
            Errors = new List<string>()
        };

        try
        {
            var fichaje = await _context.FichajesTrabajadors
                .FirstOrDefaultAsync(f =>
                    f.IdTrabajador == request.TrabajadorId &&
                    f.HoraSalida == null &&
                    (f.Borrado == null || f.Borrado == false),
                    cancellationToken);

            if (fichaje == null)
            {
                result.Errors.Add("No hay un fichaje de entrada abierto para este trabajador.");
                return result;
            }

            switch (request.Accion)
            {
                case "pausar":
                    var pausaActiva = await _context.PausasFichaje
                        .AnyAsync(p =>
                            p.IdFichaje == fichaje.IdFichaje &&
                            p.HoraFin == null &&
                            (p.Borrado == null || p.Borrado == false),
                            cancellationToken);

                    if (pausaActiva)
                    {
                        result.Errors.Add("Ya hay una pausa activa. Reanuda antes de pausar de nuevo.");
                        return result;
                    }

                    var nuevaPausa = new PausaFichaje
                    {
                        IdFichaje = fichaje.IdFichaje,
                        HoraInicio = DateTime.Now,
                        HoraFin = null,
                        FechaRegistro = DateTime.Now,
                        Borrado = false
                    };

                    await _context.PausasFichaje.AddAsync(nuevaPausa, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    result.Data = "Pausa";
                    break;

                case "reanudar":
                    var pausa = await _context.PausasFichaje
                        .OrderByDescending(p => p.HoraInicio)
                        .FirstOrDefaultAsync(p =>
                            p.IdFichaje == fichaje.IdFichaje &&
                            p.HoraFin == null &&
                            (p.Borrado == null || p.Borrado == false),
                            cancellationToken);

                    if (pausa == null)
                    {
                        result.Errors.Add("No hay ninguna pausa activa para reanudar.");
                        return result;
                    }

                    pausa.HoraFin = DateTime.Now;
                    _context.PausasFichaje.Update(pausa);
                    await _context.SaveChangesAsync(cancellationToken);
                    result.Data = "Reanudado";
                    break;

                default:
                    result.Errors.Add("Acción no válida. Usa 'pausar' o 'reanudar'.");
                    break;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error al gestionar la pausa: {ex.InnerException?.Message ?? ex.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }
}
