using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO.ResponseModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Trabajadores.Command;

public class DeleteTrabajadorCommand : IRequest<SingleResult<bool>>
{
    public int IdTrabajador { get; set; }

    public DeleteTrabajadorCommand(int idTrabajador)
    {
        IdTrabajador = idTrabajador;
    }
}

internal class DeleteTrabajadorCommandHandler
    : IRequestHandler<DeleteTrabajadorCommand, SingleResult<bool>>
{
    private readonly AppDbContext _context;

    public DeleteTrabajadorCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<bool>> Handle(
        DeleteTrabajadorCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<bool>
        {
            Data = false,
            Errors = new List<string>()
        };

        try
        {
            var trabajador = await _context.Trabajadores
                .FirstOrDefaultAsync(
                    t => t.IdTrabajador == request.IdTrabajador,
                    cancellationToken);

            if (trabajador == null)
            {
                result.Errors.Add("No se ha encontrado el trabajador a borrar.");
                return result;
            }

            trabajador.Borrado = true;
            trabajador.FechaBorrado = DateTime.Now;

            _context.Trabajadores.Update(trabajador);

            await _context.SaveChangesAsync(cancellationToken);

            result.Data = true;
        }
        catch (Exception e)
        {
            result.Errors.Add(
                $"ERROR: DeleteTrabajadorCommand - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }
}