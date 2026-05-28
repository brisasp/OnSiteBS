using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Visitantes.Query;

public class GetAllVisitantesQuery : IRequest<ListResult<VisitanteDTO>>
{
}

internal class GetAllVisitantesQueryHandler
    : IRequestHandler<GetAllVisitantesQuery, ListResult<VisitanteDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAllVisitantesQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<VisitanteDTO>> Handle(
        GetAllVisitantesQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<VisitanteDTO>
        {
            Data = new List<VisitanteDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var listVisitantes = await _context.Visitantes
                .AsNoTracking()
                .Where(v => v.Borrado == null || v.Borrado == false)
                .ToListAsync(cancellationToken);

            var visitantesDescifrados = VisitanteCryptoHelper.DescifrarVisitantes(listVisitantes, _cryptoService);

            result.Data = visitantesDescifrados
                .OrderBy(v => v.Nombre)
                .ThenBy(v => v.PrimerApellido)
                .ToList();
        }
        catch (Exception e)
        {
            var errorText = $"ERROR: GetAllVisitantes - {e}";
            result.Errors.Add(errorText);
            Console.WriteLine(errorText);
        }

        return result;
    }
}