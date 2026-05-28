using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Empresas.Query;

public class GetAllETTsQuery : IRequest<ListResult<EmpresasEttDTO>>
{
}

internal class GetAllETTsQueryHandler
    : IRequestHandler<GetAllETTsQuery, ListResult<EmpresasEttDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAllETTsQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<EmpresasEttDTO>> Handle(
        GetAllETTsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<EmpresasEttDTO>
        {
            Data = new List<EmpresasEttDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var empresas = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.Borrado == null || e.Borrado == false)
                .ToListAsync(cancellationToken);

            result.Data = EmpresaEttCryptoHelper.DescifrarEmpresas(
                empresas,
                _cryptoService);
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: GetAllETTsQuery - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }
}