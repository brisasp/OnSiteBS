using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Trabajadores.Query;

public class GetAllTrabajadoresQuery : IRequest<ListResult<TrabajadoresDTO>>
{
}

internal class GetAllTrabajadoresQueryHandler
    : IRequestHandler<GetAllTrabajadoresQuery, ListResult<TrabajadoresDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAllTrabajadoresQueryHandler(
        AppDbContext context,
        ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<TrabajadoresDTO>> Handle(
        GetAllTrabajadoresQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<TrabajadoresDTO>
        {
            Data = new List<TrabajadoresDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var trabajadores = await _context.Trabajadores
                .AsNoTracking()
                .Include(t => t.IdEttNavigation)
                .ToListAsync(cancellationToken);

            result.Data = TrabajadorCryptoHelper.DescifrarTrabajadores(
                trabajadores,
                _cryptoService);
        }
        catch (Exception e)
        {
            result.Errors.Add(
                $"ERROR: GetAllTrabajadoresQuery - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }
}