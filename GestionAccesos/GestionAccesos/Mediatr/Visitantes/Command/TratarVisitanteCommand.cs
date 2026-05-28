using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Mappings;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Visitantes.Command;

public class TratarVisitanteCommand : IRequest<SingleResult<VisitanteDTO>>
{
    public VisitanteDTO VisitanteDto { get; set; }

    public TratarVisitanteCommand(VisitanteDTO visitanteDto)
    {
        VisitanteDto = visitanteDto;
    }
}

internal class TratarVisitanteCommandHandler
    : IRequestHandler<TratarVisitanteCommand, SingleResult<VisitanteDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public TratarVisitanteCommandHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<VisitanteDTO>> Handle(
        TratarVisitanteCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<VisitanteDTO>
        {
            Data = new VisitanteDTO(),
            Errors = new List<string>()
        };

        try
        {
            var correoCifrado = _cryptoService.Encrypt(request.VisitanteDto.Correo);

            var existingVisitante = await _context.Visitantes
                .FirstOrDefaultAsync(v => v.Correo == correoCifrado && (v.Borrado == null || v.Borrado == false), cancellationToken);

            if (existingVisitante != null &&
                existingVisitante.IdVisitante != request.VisitanteDto.IdVisitante)
            {
                result.Errors.Add("Ya existe un visitante con ese correo electrónico.");
                return result;
            }

            if (request.VisitanteDto.IdVisitante == 0)
            {
                var nuevoVisitante = VisitanteMapper.ToEntity(request.VisitanteDto);

                nuevoVisitante.FechaRegistro = DateTime.Now;
                nuevoVisitante.Borrado = false;
                nuevoVisitante.FechaBorrado = null;

                VisitanteCryptoHelper.CifrarVisitante(nuevoVisitante, _cryptoService);

                await _context.Visitantes.AddAsync(nuevoVisitante, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                request.VisitanteDto.IdVisitante = nuevoVisitante.IdVisitante;
                result.Data = request.VisitanteDto;
            }
            else
            {
                var visitanteExistente = await _context.Visitantes
                    .FirstOrDefaultAsync(v => v.IdVisitante == request.VisitanteDto.IdVisitante, cancellationToken);

                if (visitanteExistente == null)
                {
                    result.Errors.Add("Visitante no encontrado.");
                    return result;
                }

                VisitanteMapper.UpdateEntity(visitanteExistente, request.VisitanteDto);
                VisitanteCryptoHelper.CifrarVisitante(visitanteExistente, _cryptoService);

                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.VisitanteDto;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR al registrar o actualizar visitante: {e.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }
}