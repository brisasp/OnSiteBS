using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Visitas.Query;

public class GetVisitasHoyQuery : IRequest<ListResult<VisitaDTO>>
{
}

internal class GetVisitasHoyQueryHandler
    : IRequestHandler<GetVisitasHoyQuery, ListResult<VisitaDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetVisitasHoyQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<VisitaDTO>> Handle(
        GetVisitasHoyQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<VisitaDTO>
        {
            Data = new List<VisitaDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var desde = DateTime.Today;
            var hasta = desde.AddDays(1).AddTicks(-1);

            var listVisitas = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.IdPersonaNavigation)
                .Include(v => v.IdVisitanteNavigation)
                .Where(v =>
                    (v.Borrado == null || v.Borrado == false) &&
                    v.FechaEntrada.HasValue &&
                    v.FechaEntrada.Value >= desde &&
                    v.FechaEntrada.Value <= hasta &&
                    v.IdPersonaNavigation != null &&
                    v.IdVisitanteNavigation != null &&
                    (v.IdPersonaNavigation.Borrado == null || v.IdPersonaNavigation.Borrado == false) &&
                    (v.IdVisitanteNavigation.Borrado == null || v.IdVisitanteNavigation.Borrado == false))
                .OrderByDescending(v => v.FechaEntrada)
                .ToListAsync(cancellationToken);

            result.Data = listVisitas.Select(v =>
            {
                var visitanteDescifrado = VisitanteCryptoHelper.DescifrarVisitante(
                    v.IdVisitanteNavigation!,
                    _cryptoService);

                var personaDescifrada = PersonaAVisitarCryptoHelper.DescifrarPersona(
                    v.IdPersonaNavigation!,
                    _cryptoService);

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
            result.Errors.Add($"ERROR: GetVisitasHoyQuery - {(e.InnerException?.Message ?? e.Message)}");
        }

        return result;
    }
}