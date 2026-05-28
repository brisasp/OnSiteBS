using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.PersonasAVisitar.Query;

public class GetAllPersonasAVisitarQuery : IRequest<ListResult<PersonasAvisitarDTO>>
{
}

internal class GetAllPersonasAVisitarQueryHandler
    : IRequestHandler<GetAllPersonasAVisitarQuery, ListResult<PersonasAvisitarDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetAllPersonasAVisitarQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<ListResult<PersonasAvisitarDTO>> Handle(
        GetAllPersonasAVisitarQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<PersonasAvisitarDTO>
        {
            Data = new List<PersonasAvisitarDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var lista = await _context.PersonasAvisitars
                .AsNoTracking()
                //.Where(p => !p.Borrado)
                .ToListAsync(cancellationToken);

            result.Data = PersonaAVisitarCryptoHelper.DescifrarPersonas(lista, _cryptoService);
        }
        catch (Exception e)
        {
            var errorText = $"ERROR: GetAllPersonasAVisitar - {(e.InnerException?.Message ?? e.Message)}";
            result.Errors.Add(errorText);
        }

        return result;
    }
}