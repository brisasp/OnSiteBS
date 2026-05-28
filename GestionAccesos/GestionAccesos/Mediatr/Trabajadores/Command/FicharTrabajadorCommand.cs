using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Trabajadores.Command;

public class FicharTrabajadorCommand : IRequest<SingleResult<string>>
{
    public FicharTrabajadorCommand(int trabajadorId)
    {
        TrabajadorId = trabajadorId;
    }

    public int TrabajadorId { get; }
}

internal class FicharTrabajadorCommandHandler
    : IRequestHandler<FicharTrabajadorCommand, SingleResult<string>>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public FicharTrabajadorCommandHandler(
        IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<SingleResult<string>> Handle(
        FicharTrabajadorCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<string>
        {
            Data = string.Empty,
            Errors = new List<string>()
        };

        try
        {
            await using var dbContext =
                await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existeTrabajador = await dbContext.Trabajadores
                .AnyAsync(t =>
                    t.IdTrabajador == request.TrabajadorId &&
                    (t.Borrado == null || t.Borrado == false),
                    cancellationToken);

            if (!existeTrabajador)
            {
                result.Errors.Add("Trabajador no encontrado");
                return result;
            }

            // Cerrar ausencia abierta
            var ausenciaAbierta = await dbContext.AusenciasTrabajadors
                .FirstOrDefaultAsync(a =>
                    a.IdTrabajador == request.TrabajadorId &&
                    a.HoraFin == null &&
                    (a.Borrado == null || a.Borrado == false),
                    cancellationToken);

            if (ausenciaAbierta != null)
            {
                ausenciaAbierta.HoraFin = DateTime.Now;

                dbContext.AusenciasTrabajadors.Update(ausenciaAbierta);

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Buscar último fichaje
            var lastFichaje = await dbContext.FichajesTrabajadors
                .Where(f =>
                    f.IdTrabajador == request.TrabajadorId &&
                    (f.Borrado == null || f.Borrado == false))
                .OrderByDescending(f => f.HoraEntrada)
                .FirstOrDefaultAsync(cancellationToken);

            // Si hay fichaje abierto => salida
            if (lastFichaje != null && lastFichaje.HoraSalida == null)
            {
                lastFichaje.HoraSalida = DateTime.Now;

                dbContext.FichajesTrabajadors.Update(lastFichaje);

                result.Data = "Salida";
            }
            else
            {
                // Nuevo fichaje => entrada
                var nuevoFichaje = new FichajesTrabajador
                {
                    IdTrabajador = request.TrabajadorId,
                    HoraEntrada = DateTime.Now,
                    HoraSalida = null,
                    FechaRegistro = DateTime.Now,
                    Borrado = false
                };

                await dbContext.FichajesTrabajadors
                    .AddAsync(nuevoFichaje, cancellationToken);

                result.Data = "Entrada";
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add(
                $"Error al fichar: {ex.InnerException?.Message ?? ex.Message}");
        }

        return result;
    }
}