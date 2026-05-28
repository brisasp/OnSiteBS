using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Fichaje.Query;

public class GetFichajeAbiertoByTrabajadorIdQuery(int trabajadorId)
    : IRequest<SingleResult<FichajesTrabajadorDTO>>
{
    public int TrabajadorId { get; set; } = trabajadorId;
}

internal class GetFichajeAbiertoByTrabajadorIdQueryHandler
    : IRequestHandler<GetFichajeAbiertoByTrabajadorIdQuery, SingleResult<FichajesTrabajadorDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetFichajeAbiertoByTrabajadorIdQueryHandler(
        AppDbContext context,
        ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<FichajesTrabajadorDTO>> Handle(
        GetFichajeAbiertoByTrabajadorIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<FichajesTrabajadorDTO>
        {
            Data = null,
            Errors = new List<string>()
        };

        try
        {
            var fichajeAbierto = await _context.FichajesTrabajadors
                .AsNoTracking()
                .Include(f => f.IdTrabajadorNavigation)
                .Where(f =>
                    f.IdTrabajador == request.TrabajadorId &&
                    f.HoraSalida == null &&
                    (f.Borrado == null || f.Borrado == false))
                .OrderByDescending(f => f.HoraEntrada)
                .FirstOrDefaultAsync(cancellationToken);

            if (fichajeAbierto == null)
            {
                result.Errors.Add("No se encontró ningún fichaje abierto para el trabajador.");
                return result;
            }

            var trabajador = fichajeAbierto.IdTrabajadorNavigation;

            var nombreCompleto = trabajador != null
                ? $"{DecryptSafe(trabajador.Nombre)} {DecryptSafe(trabajador.Apellido1)} {DecryptSafe(trabajador.Apellido2)}".Trim()
                : "Desconocido";

            result.Data = new FichajesTrabajadorDTO
            {
                IdFichaje = fichajeAbierto.IdFichaje,
                IdTrabajador = fichajeAbierto.IdTrabajador ?? 0,
                HoraEntrada = fichajeAbierto.HoraEntrada ?? DateTime.MinValue,
                HoraSalida = fichajeAbierto.HoraSalida,
                Borrado = fichajeAbierto.Borrado.GetValueOrDefault(),
                FechaRegistro = fichajeAbierto.FechaRegistro,
                FechaBorrado = fichajeAbierto.FechaBorrado,
                NombreCompleto = nombreCompleto
            };
        }
        catch (Exception e)
        {
            result.Errors.Add(
                $"ERROR: GetFichajeAbiertoByTrabajadorId - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }

    private string DecryptSafe(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : _cryptoService.Decrypt(value);
    }
}