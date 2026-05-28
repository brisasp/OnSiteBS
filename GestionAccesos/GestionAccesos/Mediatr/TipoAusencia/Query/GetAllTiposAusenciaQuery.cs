using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.TiposAusencias.Query;

public class GetAllTiposAusenciaQuery : IRequest<ListResult<TiposAusenciumDTO>>
{
}

internal class GetAllTiposAusenciaQueryHandler
    : IRequestHandler<GetAllTiposAusenciaQuery, ListResult<TiposAusenciumDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAllTiposAusenciaQueryHandler(
        AppDbContext context,
        ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<TiposAusenciumDTO>> Handle(
        GetAllTiposAusenciaQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<TiposAusenciumDTO>
        {
            Data = new List<TiposAusenciumDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var listTipos = await _context.TiposAusencia
                .AsNoTracking()
                .Where(t => t.Borrado == null || t.Borrado == false)
                .ToListAsync(cancellationToken);

            result.Data = TipoAusenciaCryptoHelper.DescifrarTipos(
                listTipos,
                _cryptoService);
        }
        catch (Exception e)
        {
            result.Errors.Add(
                $"ERROR: GetAllTiposAusenciaQuery - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }
}