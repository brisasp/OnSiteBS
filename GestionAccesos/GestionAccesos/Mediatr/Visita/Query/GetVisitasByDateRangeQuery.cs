using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Visitas.Query;

public class GetVisitasByDateRangeQuery : IRequest<ListResult<VisitaDTO>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public GetVisitasByDateRangeQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

internal class GetVisitasByDateRangeQueryHandler
    : IRequestHandler<GetVisitasByDateRangeQuery, ListResult<VisitaDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetVisitasByDateRangeQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<VisitaDTO>> Handle(
        GetVisitasByDateRangeQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<VisitaDTO>
        {
            Data = new List<VisitaDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var startDate = request.StartDate.Date;
            var endDateAdjusted = request.EndDate.Date.AddDays(1).AddTicks(-1);

            var listVisitas = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.IdPersonaNavigation)
                .Include(v => v.IdVisitanteNavigation)
                .Where(v =>
                    (v.Borrado == null || v.Borrado == false) &&
                    v.FechaEntrada.HasValue &&
                    v.FechaEntrada.Value >= startDate &&
                    v.FechaEntrada.Value <= endDateAdjusted)
                .OrderByDescending(v => v.FechaEntrada)
                .ToListAsync(cancellationToken);

            result.Data = listVisitas.Select(v =>
            {
                var visitanteDescifrado = v.IdVisitanteNavigation != null
                    ? VisitanteCryptoHelper.DescifrarVisitante(v.IdVisitanteNavigation, _cryptoService)
                    : new VisitanteDTO();

                var personaDescifrada = v.IdPersonaNavigation != null
                    ? PersonaAVisitarCryptoHelper.DescifrarPersona(v.IdPersonaNavigation, _cryptoService)
                    : new PersonasAvisitarDTO();

                return new VisitaDTO
                {
                    IdVisita = v.IdVisita,
                    IdVisitante = v.IdVisitante ?? 0,
                    IdPersona = v.IdPersona ?? 0,
                    FechaEntrada = v.FechaEntrada ?? DateTime.MinValue,
                    FechaSalida = v.FechaSalida,
                    FechaRegistro = v.FechaRegistro ?? DateTime.Now,
                    FechaBorrado = v.FechaBorrado,
                    EmpresaVisitante = visitanteDescifrado.Empresa,
                    TelefonoVisitante = visitanteDescifrado.Telefono,
                    Borrado = v.Borrado.GetValueOrDefault(),
                    IdVisitanteNavigation = visitanteDescifrado,
                    IdPersonaNavigation = personaDescifrada
                };
            }).ToList();
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: GetVisitasByDateRangeQuery - {(e.InnerException?.Message ?? e.Message)}");
        }

        return result;
    }
}