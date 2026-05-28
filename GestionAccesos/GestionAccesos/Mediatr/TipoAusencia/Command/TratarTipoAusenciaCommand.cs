using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using GestionAccesos.Helpers;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.TiposAusencias.Command;

public class TratarTipoAusenciaCommand : IRequest<SingleResult<TiposAusenciumDTO>>
{
    public TiposAusenciumDTO Dto { get; set; }
    public bool Eliminar { get; set; }

    public TratarTipoAusenciaCommand(TiposAusenciumDTO dto, bool eliminar = false)
    {
        Dto = dto;
        Eliminar = eliminar;
    }
}

internal class TratarTipoAusenciaCommandHandler
    : IRequestHandler<TratarTipoAusenciaCommand, SingleResult<TiposAusenciumDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public TratarTipoAusenciaCommandHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<TiposAusenciumDTO>> Handle(
        TratarTipoAusenciaCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<TiposAusenciumDTO>
        {
            Data = new TiposAusenciumDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.Eliminar)
            {
                var existente = await _context.TiposAusencia
                    .FirstOrDefaultAsync(t => t.IdTipoAusencia == request.Dto.IdTipoAusencia, cancellationToken);

                if (existente == null)
                {
                    result.Errors.Add("Tipo de ausencia no encontrado.");
                    return result;
                }

                existente.Borrado = true;
                existente.FechaBorrado = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.Dto;
            }
            else if (request.Dto.IdTipoAusencia == 0)
            {
                var nuevo = TipoAusenciaCryptoHelper.CifrarTipoDTO(request.Dto, _cryptoService);
                nuevo.FechaRegistro = DateTime.Now;
                nuevo.Borrado = false;
                nuevo.Activo = true;

                await _context.TiposAusencia.AddAsync(nuevo, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                request.Dto.IdTipoAusencia = nuevo.IdTipoAusencia;
                request.Dto.FechaRegistro = nuevo.FechaRegistro;
                result.Data = request.Dto;
            }
            else
            {
                var existente = await _context.TiposAusencia
                    .FirstOrDefaultAsync(t => t.IdTipoAusencia == request.Dto.IdTipoAusencia, cancellationToken);

                if (existente == null)
                {
                    result.Errors.Add("Tipo de ausencia no encontrado.");
                    return result;
                }

                existente.Descripcion = string.IsNullOrWhiteSpace(request.Dto.Descripcion)
                    ? string.Empty
                    : _cryptoService.Encrypt(request.Dto.Descripcion);
                existente.Activo = request.Dto.Activo;

                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.Dto;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR al tratar tipo de ausencia: {e.InnerException?.Message ?? e.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }
}
