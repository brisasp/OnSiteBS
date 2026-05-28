using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Visita.Command;

public class TratarVisitaCommand : IRequest<SingleResult<VisitaDTO>>
{
    public VisitaDTO VisitaDto { get; set; }

    public TratarVisitaCommand(VisitaDTO visitaDto)
    {
        VisitaDto = visitaDto;
    }
}

internal class TratarVisitaCommandHandler
    : IRequestHandler<TratarVisitaCommand, SingleResult<VisitaDTO>>
{
    private readonly AppDbContext _context;

    public TratarVisitaCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<VisitaDTO>> Handle(
        TratarVisitaCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<VisitaDTO>
        {
            Data = new VisitaDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.VisitaDto.IdVisita > 0)
            {
                var visitaPorId = await _context.Visitas
                    .FirstOrDefaultAsync(v =>
                        v.IdVisita == request.VisitaDto.IdVisita &&
                        v.FechaSalida == null,
                        cancellationToken);

                if (visitaPorId != null)
                {
                    visitaPorId.FechaSalida = DateTime.Now;
                    _context.Visitas.Update(visitaPorId);
                    await _context.SaveChangesAsync(cancellationToken);

                    request.VisitaDto.FechaEntrada = visitaPorId.FechaEntrada ?? DateTime.MinValue;
                    request.VisitaDto.FechaSalida = visitaPorId.FechaSalida;
                    request.VisitaDto.FechaRegistro = visitaPorId.FechaRegistro ?? DateTime.Now;
                    request.VisitaDto.Borrado = visitaPorId.Borrado.GetValueOrDefault();
                    result.Data = request.VisitaDto;

                    _context.ChangeTracker.Clear();
                    return result;
                }
            }

            var visitaExistente = await _context.Visitas
                .FirstOrDefaultAsync(v =>
                    v.IdVisitante == request.VisitaDto.IdVisitante &&
                    v.IdPersona == request.VisitaDto.IdPersona &&
                    v.FechaSalida == null,
                    cancellationToken);

            if (visitaExistente != null)
            {
                visitaExistente.FechaSalida = DateTime.Now;

                _context.Visitas.Update(visitaExistente);
                await _context.SaveChangesAsync(cancellationToken);

                request.VisitaDto.IdVisita = visitaExistente.IdVisita;
                request.VisitaDto.FechaEntrada = visitaExistente.FechaEntrada ?? DateTime.MinValue;
                request.VisitaDto.FechaSalida = visitaExistente.FechaSalida;
                request.VisitaDto.FechaRegistro = visitaExistente.FechaRegistro ?? DateTime.Now;
                request.VisitaDto.Borrado = visitaExistente.Borrado.GetValueOrDefault();

                result.Data = request.VisitaDto;
            }
            else
            {
                var ahora = DateTime.Now;

                var nuevaVisita = new Entities.Visita
                {
                    IdVisitante = request.VisitaDto.IdVisitante,
                    IdPersona = request.VisitaDto.IdPersona,
                    FechaEntrada = request.VisitaDto.FechaEntrada == default
                        ? ahora
                        : request.VisitaDto.FechaEntrada,
                    FechaRegistro = ahora,
                    Borrado = false
                };

                await _context.Visitas.AddAsync(nuevaVisita, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                request.VisitaDto.IdVisita = nuevaVisita.IdVisita;
                request.VisitaDto.FechaEntrada = nuevaVisita.FechaEntrada ?? DateTime.MinValue;
                request.VisitaDto.FechaRegistro = nuevaVisita.FechaRegistro ?? ahora;
                request.VisitaDto.Borrado = nuevaVisita.Borrado.GetValueOrDefault();

                result.Data = request.VisitaDto;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR al guardar la visita: {e.InnerException?.Message ?? e.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }
}