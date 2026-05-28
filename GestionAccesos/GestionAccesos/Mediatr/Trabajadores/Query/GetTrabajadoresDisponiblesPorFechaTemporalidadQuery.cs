using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Trabajadores.Query;

public class GetTrabajadoresDisponiblesPorFechaTemporalidadQuery : IRequest<ListResult<TrabajadoresDTO>>
{
}

internal class GetTrabajadoresDisponiblesPorFechaTemporalidadQueryHandler
    : IRequestHandler<GetTrabajadoresDisponiblesPorFechaTemporalidadQuery, ListResult<TrabajadoresDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetTrabajadoresDisponiblesPorFechaTemporalidadQueryHandler(
        AppDbContext context,
        ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<TrabajadoresDTO>> Handle(
        GetTrabajadoresDisponiblesPorFechaTemporalidadQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<TrabajadoresDTO>
        {
            Data = new List<TrabajadoresDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var today = DateTime.Now;

            var trabajadoresDisponibles = await _context.Trabajadores
                .AsNoTracking()
                .Include(t => t.IdEttNavigation)
                .Where(t =>
                    t.FechaMaximaTemporalidad.HasValue &&
                    t.FechaMaximaTemporalidad.Value > today &&
                    (t.Borrado == null || t.Borrado == false))
                .ToListAsync(cancellationToken);

            result.Data = TrabajadorCryptoHelper.DescifrarTrabajadores(
                trabajadoresDisponibles,
                _cryptoService);
        }
        catch (Exception ex)
        {
            result.Errors.Add(
                $"ERROR: GetTrabajadoresDisponiblesPorFechaTemporalidadQuery - {ex.InnerException?.Message ?? ex.Message}");
        }

        return result;
    }
}