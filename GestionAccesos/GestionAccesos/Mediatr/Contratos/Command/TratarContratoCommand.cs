using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Contratos.Command;

public class TratarContratoCommand : IRequest<SingleResult<ContratosEttDTO>>
{
    public ContratosEttDTO ContratoDto { get; set; }
    public bool Eliminar { get; set; }

    public TratarContratoCommand(ContratosEttDTO contratoDto, bool eliminar = false)
    {
        ContratoDto = contratoDto;
        Eliminar = eliminar;
    }
}

internal class TratarContratoCommandHandler
    : IRequestHandler<TratarContratoCommand, SingleResult<ContratosEttDTO>>
{
    private readonly AppDbContext _context;

    public TratarContratoCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<ContratosEttDTO>> Handle(
        TratarContratoCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<ContratosEttDTO>
        {
            Data = new ContratosEttDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.Eliminar)
            {
                var contrato = await _context.ContratosTrabajadores
                    .FirstOrDefaultAsync(c => c.IdContrato == request.ContratoDto.IdContrato, cancellationToken);

                if (contrato == null)
                {
                    result.Errors.Add("Contrato no encontrado.");
                    return result;
                }

                contrato.Borrado = true;
                contrato.FechaBorrado = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.ContratoDto;
            }
            else if (request.ContratoDto.IdContrato == 0)
            {
                var nuevo = new ContratosTrabajadore
                {
                    IdTrabajador = request.ContratoDto.IdTrabajador,
                    FechaInicioContrato = request.ContratoDto.FechaInicioContrato,
                    FechaFinContrato = request.ContratoDto.FechaFinContrato,
                    FechaRegistro = DateTime.Now,
                    Borrado = false
                };

                await _context.ContratosTrabajadores.AddAsync(nuevo, cancellationToken);

                await ActualizarFechaMaximaTemporalidad(request.ContratoDto.IdTrabajador,
                    request.ContratoDto.FechaFinContrato, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                request.ContratoDto.IdContrato = nuevo.IdContrato;
                request.ContratoDto.FechaRegistro = nuevo.FechaRegistro ?? DateTime.Now;
                result.Data = request.ContratoDto;
            }
            else
            {
                var existente = await _context.ContratosTrabajadores
                    .FirstOrDefaultAsync(c => c.IdContrato == request.ContratoDto.IdContrato, cancellationToken);

                if (existente == null)
                {
                    result.Errors.Add("Contrato no encontrado.");
                    return result;
                }

                existente.IdTrabajador = request.ContratoDto.IdTrabajador;
                existente.FechaInicioContrato = request.ContratoDto.FechaInicioContrato;
                existente.FechaFinContrato = request.ContratoDto.FechaFinContrato;
                existente.FechaBaja = request.ContratoDto.FechaBaja;
                existente.MotivoBaja = request.ContratoDto.MotivoBaja;

                await ActualizarFechaMaximaTemporalidad(request.ContratoDto.IdTrabajador,
                    request.ContratoDto.FechaFinContrato, cancellationToken);

                if (request.ContratoDto.FechaBaja.HasValue)
                    await MarcarTrabajadorComoBaja(request.ContratoDto.IdTrabajador, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.ContratoDto;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR al tratar contrato: {e.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }

    private async Task ActualizarFechaMaximaTemporalidad(int idTrabajador, DateTime fechaFin, CancellationToken ct)
    {
        var trabajador = await _context.Trabajadores
            .FirstOrDefaultAsync(t => t.IdTrabajador == idTrabajador, ct);

        if (trabajador == null) return;

        if (!trabajador.FechaMaximaTemporalidad.HasValue || fechaFin > trabajador.FechaMaximaTemporalidad.Value)
            trabajador.FechaMaximaTemporalidad = fechaFin;
    }

    private async Task MarcarTrabajadorComoBaja(int idTrabajador, CancellationToken ct)
    {
        var trabajador = await _context.Trabajadores
            .FirstOrDefaultAsync(t => t.IdTrabajador == idTrabajador, ct);

        if (trabajador == null) return;

        trabajador.Borrado = true;
        trabajador.FechaBorrado = DateTime.Now;
    }
}
