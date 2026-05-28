using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Trabajadores.Query;

public class GetTrabajadorByIdQuery : IRequest<SingleResult<TrabajadoresDTO>>
{
    public int TrabajadorId { get; }

    public GetTrabajadorByIdQuery(int trabajadorId)
    {
        TrabajadorId = trabajadorId;
    }
}

internal class GetTrabajadorByIdQueryHandler
    : IRequestHandler<GetTrabajadorByIdQuery, SingleResult<TrabajadoresDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetTrabajadorByIdQueryHandler(
        AppDbContext context,
        ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<TrabajadoresDTO>> Handle(
        GetTrabajadorByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<TrabajadoresDTO>
        {
            Data = null,
            Errors = new List<string>()
        };

        try
        {
            var trabajador = await _context.Trabajadores
                .AsNoTracking()
                .Include(t => t.IdEttNavigation)
                .FirstOrDefaultAsync(
                    t => t.IdTrabajador == request.TrabajadorId,
                    cancellationToken);

            if (trabajador == null)
            {
                result.Errors.Add(
                    $"ERROR: No se encontró el trabajador con ID {request.TrabajadorId}");
            }
            else
            {
                result.Data = TrabajadorCryptoHelper.DescifrarTrabajador(
                    trabajador,
                    _cryptoService);
            }
        }
        catch (Exception e)
        {
            var errorText =
                $"ERROR: GetTrabajadorById - {(e.InnerException?.Message ?? e.Message)}";

            result.Errors.Add(errorText);
        }

        return result;
    }
}