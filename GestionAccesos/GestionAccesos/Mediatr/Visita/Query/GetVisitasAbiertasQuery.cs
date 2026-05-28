using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Visitas.Query;

public class GetVisitasAbiertasQuery : IRequest<ListResult<VisitaDTO>>
{
}

internal class GetVisitasAbiertasQueryHandler
    : IRequestHandler<GetVisitasAbiertasQuery, ListResult<VisitaDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetVisitasAbiertasQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<VisitaDTO>> Handle(
        GetVisitasAbiertasQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<VisitaDTO>
        {
            Data = new List<VisitaDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var listVisitas = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.IdPersonaNavigation)
                .Include(v => v.IdVisitanteNavigation)
                .Where(v =>
                    (v.Borrado == null || v.Borrado == false) &&
                    v.FechaEntrada != null &&
                    v.FechaSalida == null)
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
            result.Errors.Add($"ERROR: GetVisitasAbiertasQuery - {(e.InnerException?.Message ?? e.Message)}");
        }

        return result;
    }
}