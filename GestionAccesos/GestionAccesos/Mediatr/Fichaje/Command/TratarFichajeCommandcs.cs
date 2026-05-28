using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Fichaje.Command;

public class TratarFichajeCommand(FichajesTrabajadorDTO fichajeDTO, int idFichaje)
    : IRequest<SingleResult<FichajesTrabajadorDTO>>
{
    public FichajesTrabajadorDTO FichajeDTO { get; set; } = fichajeDTO;
    public int IdFichaje { get; set; } = idFichaje;
}

internal class TratarFichajeCommandHandler(AppDbContext context)
    : IRequestHandler<TratarFichajeCommand, SingleResult<FichajesTrabajadorDTO>>
{
    public async Task<SingleResult<FichajesTrabajadorDTO>> Handle(
        TratarFichajeCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<FichajesTrabajadorDTO>
        {
            Data = new FichajesTrabajadorDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.IdFichaje == 0)
            {
                var trabajadorExistente = await context.Trabajadores
                    .FirstOrDefaultAsync(t =>
                        t.IdTrabajador == request.FichajeDTO.IdTrabajador &&
                        (t.Borrado == null || t.Borrado == false),
                        cancellationToken);

                if (trabajadorExistente == null)
                {
                    result.Errors.Add("El trabajador especificado no existe.");
                    return result;
                }

                var nuevoFichaje = new FichajesTrabajador
                {
                    IdTrabajador = request.FichajeDTO.IdTrabajador,
                    HoraEntrada = request.FichajeDTO.HoraEntrada == default
                        ? DateTime.Now
                        : request.FichajeDTO.HoraEntrada,
                    HoraSalida = request.FichajeDTO.HoraSalida,
                    FechaRegistro = request.FichajeDTO.FechaRegistro ?? DateTime.Now,
                    FechaBorrado = request.FichajeDTO.FechaBorrado,
                    Borrado = request.FichajeDTO.Borrado,
                    IdTrabajadorNavigation = trabajadorExistente
                };

                await context.FichajesTrabajadors.AddAsync(nuevoFichaje, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                request.FichajeDTO.IdFichaje = nuevoFichaje.IdFichaje;
                request.FichajeDTO.IdTrabajador = nuevoFichaje.IdTrabajador ?? 0;
                request.FichajeDTO.HoraEntrada = nuevoFichaje.HoraEntrada ?? DateTime.MinValue;
                request.FichajeDTO.HoraSalida = nuevoFichaje.HoraSalida;
                request.FichajeDTO.FechaRegistro = nuevoFichaje.FechaRegistro;
                request.FichajeDTO.Borrado = nuevoFichaje.Borrado.GetValueOrDefault();

                result.Data = request.FichajeDTO;
            }
            else
            {
                var currentFichaje = await context.FichajesTrabajadors
                    .FirstOrDefaultAsync(f => f.IdFichaje == request.IdFichaje, cancellationToken);

                if (currentFichaje == null)
                {
                    result.Errors.Add("Fichaje no encontrado.");
                    return result;
                }

                currentFichaje.IdTrabajador = request.FichajeDTO.IdTrabajador;
                currentFichaje.HoraEntrada = request.FichajeDTO.HoraEntrada;
                currentFichaje.HoraSalida = request.FichajeDTO.HoraSalida;
                currentFichaje.FechaBorrado = request.FichajeDTO.FechaBorrado;
                currentFichaje.Borrado = request.FichajeDTO.Borrado;

                await context.SaveChangesAsync(cancellationToken);

                request.FichajeDTO.FechaRegistro = currentFichaje.FechaRegistro;
                result.Data = request.FichajeDTO;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: GrabarFichaje - {e.InnerException?.Message ?? e.Message}");
        }

        context.ChangeTracker.Clear();
        return result;
    }
}