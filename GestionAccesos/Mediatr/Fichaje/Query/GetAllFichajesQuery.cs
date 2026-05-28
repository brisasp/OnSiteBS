using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Fichaje.Query;

public class GetAllFichajesQuery : IRequest<ListResult<FichajesTrabajadorDTO>>
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public bool SoloAbiertos { get; set; }
}

internal class GetAllFichajesQueryHandler
    : IRequestHandler<GetAllFichajesQuery, ListResult<FichajesTrabajadorDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAllFichajesQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<FichajesTrabajadorDTO>> Handle(
        GetAllFichajesQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<FichajesTrabajadorDTO>
        {
            Data = new List<FichajesTrabajadorDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var query = _context.FichajesTrabajadors
                .AsNoTracking()
                .Include(f => f.IdTrabajadorNavigation)
                .Where(f => f.Borrado == null || f.Borrado == false);

            if (request.SoloAbiertos)
                query = query.Where(f => f.HoraSalida == null);

            if (request.Desde.HasValue)
                query = query.Where(f => f.HoraEntrada >= request.Desde.Value);

            if (request.Hasta.HasValue)
                query = query.Where(f => f.HoraEntrada <= request.Hasta.Value);

            var fichajes = await query
                .OrderByDescending(f => f.HoraEntrada)
                .ToListAsync(cancellationToken);

            result.Data = fichajes.Select(f =>
            {
                var t = f.IdTrabajadorNavigation;
                var nombre = t != null
                    ? $"{DecryptSafe(t.Nombre)} {DecryptSafe(t.Apellido1)} {DecryptSafe(t.Apellido2)}".Trim()
                    : "Desconocido";

                return new FichajesTrabajadorDTO
                {
                    IdFichaje = f.IdFichaje,
                    IdTrabajador = f.IdTrabajador ?? 0,
                    HoraEntrada = f.HoraEntrada,
                    HoraSalida = f.HoraSalida,
                    Borrado = f.Borrado.GetValueOrDefault(),
                    FechaRegistro = f.FechaRegistro,
                    FechaBorrado = f.FechaBorrado,
                    NombreCompleto = nombre,
                    Departamento = t?.Departamento != null ? DecryptSafe(t.Departamento) : null,
                    Observaciones = t?.Observaciones != null ? DecryptSafe(t.Observaciones) : null
                };
            }).ToList();
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: GetAllFichajes - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }

    private string DecryptSafe(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : _cryptoService.Decrypt(value);
}
