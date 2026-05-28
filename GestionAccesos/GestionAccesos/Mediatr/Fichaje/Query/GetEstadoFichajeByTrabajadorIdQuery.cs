using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Fichaje.Query;

public class GetEstadoFichajeByTrabajadorIdQuery : IRequest<SingleResult<EstadoFichajeDTO>>
{
    public int TrabajadorId { get; set; }
}

public class EstadoFichajeDTO
{
    public int? IdFichaje { get; set; }
    public bool TieneFichajeAbierto { get; set; }
    public bool TienePausaActiva { get; set; }
    public DateTime? HoraEntrada { get; set; }
    public DateTime? InicioUltimaPausa { get; set; }
}

internal class GetEstadoFichajeByTrabajadorIdQueryHandler
    : IRequestHandler<GetEstadoFichajeByTrabajadorIdQuery, SingleResult<EstadoFichajeDTO>>
{
    private readonly AppDbContext _context;

    public GetEstadoFichajeByTrabajadorIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<EstadoFichajeDTO>> Handle(
        GetEstadoFichajeByTrabajadorIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<EstadoFichajeDTO>
        {
            Data = new EstadoFichajeDTO(),
            Errors = new List<string>()
        };

        try
        {
            var fichaje = await _context.FichajesTrabajadors
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.IdTrabajador == request.TrabajadorId &&
                    f.HoraSalida == null &&
                    (f.Borrado == null || f.Borrado == false),
                    cancellationToken);

            if (fichaje == null)
            {
                result.Data = new EstadoFichajeDTO { TieneFichajeAbierto = false };
                return result;
            }

            // La tabla PausaFichaje puede no existir aún; la tratamos de forma aislada
            PausaFichaje? pausa = null;
            try
            {
                pausa = await _context.PausasFichaje
                    .AsNoTracking()
                    .OrderByDescending(p => p.HoraInicio)
                    .FirstOrDefaultAsync(p =>
                        p.IdFichaje == fichaje.IdFichaje &&
                        p.HoraFin == null &&
                        (p.Borrado == null || p.Borrado == false),
                        cancellationToken);
            }
            catch
            {
                // tabla aún no creada en la BD: ignoramos pausas
            }

            result.Data = new EstadoFichajeDTO
            {
                IdFichaje = fichaje.IdFichaje,
                TieneFichajeAbierto = true,
                TienePausaActiva = pausa != null,
                HoraEntrada = fichaje.HoraEntrada,
                InicioUltimaPausa = pausa?.HoraInicio
            };
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error al consultar estado de fichaje: {ex.InnerException?.Message ?? ex.Message}");
        }

        return result;
    }
}
