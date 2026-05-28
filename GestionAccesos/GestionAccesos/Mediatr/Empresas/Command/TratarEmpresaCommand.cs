using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Empresas.Command;

public class TratarEmpresaCommand(EmpresasEttDTO dto, bool eliminar = false)
    : IRequest<SingleResult<EmpresasEttDTO>>
{
    public EmpresasEttDTO Dto { get; set; } = dto;
    public bool Eliminar { get; set; } = eliminar;
}

internal class TratarEmpresaCommandHandler
    : IRequestHandler<TratarEmpresaCommand, SingleResult<EmpresasEttDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public TratarEmpresaCommandHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<EmpresasEttDTO>> Handle(
        TratarEmpresaCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<EmpresasEttDTO>
        {
            Data = new EmpresasEttDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.Dto.IdEtt == 0)
            {
                var nueva = EmpresaEttCryptoHelper.CifrarEmpresaDTO(request.Dto, _cryptoService);
                nueva.FechaRegistro = DateTime.Now;
                nueva.Borrado = false;

                await _context.Empresas.AddAsync(nueva, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                request.Dto.IdEtt = nueva.IdEtt;
                request.Dto.FechaRegistro = nueva.FechaRegistro;
                result.Data = request.Dto;
            }
            else if (request.Eliminar)
            {
                var existente = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.IdEtt == request.Dto.IdEtt, cancellationToken);

                if (existente == null)
                {
                    result.Errors.Add("Empresa no encontrada.");
                    return result;
                }

                existente.Borrado = true;
                existente.FechaBorrado = DateTime.Now;

                await _context.SaveChangesAsync(cancellationToken);
                result.Data = request.Dto;
            }
            else
            {
                var existente = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.IdEtt == request.Dto.IdEtt, cancellationToken);

                if (existente == null)
                {
                    result.Errors.Add("Empresa no encontrada.");
                    return result;
                }

                var actualizada = EmpresaEttCryptoHelper.CifrarEmpresaDTO(request.Dto, _cryptoService);
                actualizada.IdEtt = existente.IdEtt;
                actualizada.FechaRegistro = existente.FechaRegistro;

                _context.Entry(existente).CurrentValues.SetValues(actualizada);
                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.Dto;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: TratarEmpresa - {e.InnerException?.Message ?? e.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }
}
