using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Trabajadores.Query;

public class GetTrabajadoresByDateRangeQuery : IRequest<ListResult<TrabajadoresDTO>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public GetTrabajadoresByDateRangeQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

internal class GetTrabajadoresByDateRangeQueryHandler
    : IRequestHandler<GetTrabajadoresByDateRangeQuery, ListResult<TrabajadoresDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetTrabajadoresByDateRangeQueryHandler(
        AppDbContext context,
        ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<TrabajadoresDTO>> Handle(
        GetTrabajadoresByDateRangeQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<TrabajadoresDTO>
        {
            Data = new List<TrabajadoresDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var startDate = request.StartDate.Date;
            var endDateAdjusted = request.EndDate.Date.AddDays(1).AddTicks(-1);

            var trabajadoresFiltrados = await _context.Trabajadores
                .AsNoTracking()
                .Include(t => t.IdEttNavigation)
                .Where(t =>
                    t.FechaMaximaTemporalidad.HasValue &&
                    t.FechaMaximaTemporalidad.Value >= startDate &&
                    t.FechaMaximaTemporalidad.Value <= endDateAdjusted)
                .ToListAsync(cancellationToken);

            result.Data = TrabajadorCryptoHelper.DescifrarTrabajadores(
                trabajadoresFiltrados,
                _cryptoService);
        }
        catch (Exception e)
        {
            result.Errors.Add(
                $"ERROR: GetTrabajadoresByDateRangeQuery - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }
}