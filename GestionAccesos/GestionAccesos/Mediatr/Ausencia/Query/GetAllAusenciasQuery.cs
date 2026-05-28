using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Ausencia.Query;

public class GetAllAusenciasQuery : IRequest<ListResult<AusenciasTrabajadorDTO>>
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? IdTrabajador { get; set; }
    public bool SoloAbiertas { get; set; }
}

internal class GetAllAusenciasQueryHandler
    : IRequestHandler<GetAllAusenciasQuery, ListResult<AusenciasTrabajadorDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAllAusenciasQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<AusenciasTrabajadorDTO>> Handle(
        GetAllAusenciasQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<AusenciasTrabajadorDTO>
        {
            Data = new List<AusenciasTrabajadorDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var query = _context.AusenciasTrabajadors
                .AsNoTracking()
                .Include(a => a.IdTrabajadorNavigation)
                .Include(a => a.MotivoNavigation)
                .Where(a => a.Borrado == null || a.Borrado == false);

            if (request.Desde.HasValue)
                query = query.Where(a => a.HoraInicio >= request.Desde.Value);

            if (request.Hasta.HasValue)
                query = query.Where(a => a.HoraInicio <= request.Hasta.Value);

            if (request.IdTrabajador.HasValue)
                query = query.Where(a => a.IdTrabajador == request.IdTrabajador.Value);

            if (request.SoloAbiertas)
                query = query.Where(a => a.HoraFin == null);

            var ausencias = await query
                .OrderByDescending(a => a.HoraInicio)
                .ToListAsync(cancellationToken);

            result.Data = ausencias.Select(a =>
            {
                var t = a.IdTrabajadorNavigation;
                var nombreCompleto = t != null
                    ? $"{DecryptSafe(t.Nombre)} {DecryptSafe(t.Apellido1)} {DecryptSafe(t.Apellido2)}".Trim()
                    : "Desconocido";

                return new AusenciasTrabajadorDTO
                {
                    IdAusencia = a.IdAusencia,
                    IdTrabajador = a.IdTrabajador ?? 0,
                    HoraInicio = a.HoraInicio ?? DateTime.MinValue,
                    HoraFin = a.HoraFin,
                    Motivo = a.Motivo ?? 0,
                    Observaciones = DecryptSafe(a.Observaciones),
                    Borrado = a.Borrado.GetValueOrDefault(),
                    FechaRegistro = a.FechaRegistro,
                    FechaBorrado = a.FechaBorrado,
                    NombreCompleto = nombreCompleto,
                    DescripcionMotivo = DecryptSafe(a.MotivoNavigation?.Descripcion)
                };
            }).ToList();
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: GetAllAusencias - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }

    private string DecryptSafe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        try { return _cryptoService.Decrypt(value); }
        catch { return value; }
    }
}
