using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Trabajadores.Command;

public class TratarTrabajadorCommand(TrabajadoresDTO trabajadorDTO, int idTrabajador)
    : IRequest<SingleResult<TrabajadoresDTO>>
{
    public TrabajadoresDTO TrabajadorDTO { get; set; } = trabajadorDTO;
    public int IdTrabajador { get; set; } = idTrabajador;
}

internal class TratarTrabajadorCommandHandler
    : IRequestHandler<TratarTrabajadorCommand, SingleResult<TrabajadoresDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public TratarTrabajadorCommandHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<TrabajadoresDTO>> Handle(
        TratarTrabajadorCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<TrabajadoresDTO>
        {
            Data = new TrabajadoresDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.IdTrabajador == 0)
            {
                var empresaExistente = await _context.Empresas
                    .FirstOrDefaultAsync(e =>
                        e.IdEtt == request.TrabajadorDTO.IdEtt &&
                        (e.Borrado == null || e.Borrado == false),
                        cancellationToken);

                if (empresaExistente == null)
                {
                    result.Errors.Add("La empresa especificada no existe.");
                    return result;
                }

                var nuevoTrabajador = TrabajadorCryptoHelper.CifrarTrabajadorDTO(
                    request.TrabajadorDTO,
                    _cryptoService);

                nuevoTrabajador.FechaRegistro = DateTime.Now;
                nuevoTrabajador.Borrado = false;
                nuevoTrabajador.IdEttNavigation = empresaExistente;

                await _context.Trabajadores.AddAsync(nuevoTrabajador, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                request.TrabajadorDTO.IdTrabajador = nuevoTrabajador.IdTrabajador;
                request.TrabajadorDTO.FechaRegistro = nuevoTrabajador.FechaRegistro;
                request.TrabajadorDTO.Borrado = nuevoTrabajador.Borrado.GetValueOrDefault();

                result.Data = request.TrabajadorDTO;
            }
            else
            {
                var currentTrabajador = await _context.Trabajadores
                    .FirstOrDefaultAsync(t =>
                        t.IdTrabajador == request.IdTrabajador,
                        cancellationToken);

                if (currentTrabajador == null)
                {
                    result.Errors.Add("Trabajador no encontrado.");
                    return result;
                }

                var updatedTrabajador = TrabajadorCryptoHelper.CifrarTrabajadorDTO(
                    request.TrabajadorDTO,
                    _cryptoService);

                updatedTrabajador.IdTrabajador = currentTrabajador.IdTrabajador;
                updatedTrabajador.FechaRegistro = currentTrabajador.FechaRegistro;

                if (request.TrabajadorDTO.FechaMaximaTemporalidad == null)
                    updatedTrabajador.FechaMaximaTemporalidad = currentTrabajador.FechaMaximaTemporalidad;

                _context.Entry(currentTrabajador).CurrentValues.SetValues(updatedTrabajador);

                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.TrabajadorDTO;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: GrabarTrabajador - {e.InnerException?.Message ?? e.Message}");
        }

        _context.ChangeTracker.Clear();

        return result;
    }
}