using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Ausencia.Query;

public class GetAusenciaAbiertaByTrabajadorIdQuery(int trabajadorId)
    : IRequest<SingleResult<AusenciasTrabajadorDTO>>
{
    public int TrabajadorId { get; set; } = trabajadorId;
}

internal class GetAusenciaAbiertaByTrabajadorIdQueryHandler
    : IRequestHandler<GetAusenciaAbiertaByTrabajadorIdQuery, SingleResult<AusenciasTrabajadorDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAusenciaAbiertaByTrabajadorIdQueryHandler(
        AppDbContext context,
        ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<AusenciasTrabajadorDTO>> Handle(
        GetAusenciaAbiertaByTrabajadorIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<AusenciasTrabajadorDTO>
        {
            Data = null,
            Errors = new List<string>()
        };

        try
        {
            var ausenciaAbierta = await _context.AusenciasTrabajadors
                .AsNoTracking()
                .Include(a => a.IdTrabajadorNavigation)
                .Include(a => a.MotivoNavigation)
                .Where(a =>
                    a.IdTrabajador == request.TrabajadorId &&
                    a.HoraFin == null &&
                    (a.Borrado == null || a.Borrado == false))
                .OrderByDescending(a => a.HoraInicio)
                .FirstOrDefaultAsync(cancellationToken);

            if (ausenciaAbierta == null)
            {
                result.Errors.Add("No se encontró ninguna ausencia activa para el trabajador.");
                return result;
            }

            var trabajador = ausenciaAbierta.IdTrabajadorNavigation;

            result.Data = new AusenciasTrabajadorDTO
            {
                IdAusencia = ausenciaAbierta.IdAusencia,
                IdTrabajador = ausenciaAbierta.IdTrabajador ?? 0,
                HoraInicio = ausenciaAbierta.HoraInicio ?? DateTime.MinValue,
                HoraFin = ausenciaAbierta.HoraFin,
                Motivo = ausenciaAbierta.Motivo ?? 0,
                Observaciones = string.IsNullOrWhiteSpace(ausenciaAbierta.Observaciones)
                    ? null
                    : _cryptoService.Decrypt(ausenciaAbierta.Observaciones),
                Borrado = ausenciaAbierta.Borrado.GetValueOrDefault(),
                FechaRegistro = ausenciaAbierta.FechaRegistro,
                FechaBorrado = ausenciaAbierta.FechaBorrado,
                IdTrabajadorNavigation = trabajador != null
                    ? new TrabajadoresDTO
                    {
                        IdTrabajador = trabajador.IdTrabajador,
                        Nombre = string.IsNullOrWhiteSpace(trabajador.Nombre)
                            ? string.Empty
                            : _cryptoService.Decrypt(trabajador.Nombre),
                        Apellido1 = string.IsNullOrWhiteSpace(trabajador.Apellido1)
                            ? string.Empty
                            : _cryptoService.Decrypt(trabajador.Apellido1),
                        Apellido2 = string.IsNullOrWhiteSpace(trabajador.Apellido2)
                            ? null
                            : _cryptoService.Decrypt(trabajador.Apellido2),
                        Dni = string.IsNullOrWhiteSpace(trabajador.Dni)
                            ? string.Empty
                            : _cryptoService.Decrypt(trabajador.Dni),
                        Borrado = trabajador.Borrado.GetValueOrDefault()
                    }
                    : null,
                MotivoNavigation = ausenciaAbierta.MotivoNavigation != null
                    ? new TiposAusenciumDTO
                    {
                        IdTipoAusencia = ausenciaAbierta.MotivoNavigation.IdTipoAusencia,
                        Descripcion = ausenciaAbierta.MotivoNavigation.Descripcion,
                        Activo = ausenciaAbierta.MotivoNavigation.Activo,
                        Borrado = ausenciaAbierta.MotivoNavigation.Borrado.GetValueOrDefault(),
                        FechaRegistro = ausenciaAbierta.MotivoNavigation.FechaRegistro,
                        FechaBorrado = ausenciaAbierta.MotivoNavigation.FechaBorrado
                    }
                    : null
            };
        }
        catch (Exception e)
        {
            result.Errors.Add(
                $"ERROR: GetAusenciaAbiertaByTrabajadorIdQuery - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }
}