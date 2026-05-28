using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Visitantes.Query;

public class GetVisitanteByCorreoQuery : IRequest<SingleResult<VisitanteDTO>>
{
    public string CorreoVisitante { get; set; }

    public GetVisitanteByCorreoQuery(string correoVisitante)
    {
        CorreoVisitante = correoVisitante;
    }
}

internal class GetVisitanteByCorreoQueryHandler : IRequestHandler<GetVisitanteByCorreoQuery, SingleResult<VisitanteDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public GetVisitanteByCorreoQueryHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<VisitanteDTO>> Handle(GetVisitanteByCorreoQuery request, CancellationToken cancellationToken)
    {
        var result = new SingleResult<VisitanteDTO>
        {
            Data = null,
            Errors = new List<string>()
        };

        try
        {
            // Cifrar el correo de búsqueda
            var correoCifrado = _cryptoService.Encrypt(request.CorreoVisitante);

            var visitante = await _context.Visitantes
                .Where(u => u.Correo == correoCifrado && (u.Borrado == null || u.Borrado == false))
                .FirstOrDefaultAsync(cancellationToken);

            if (visitante != null)
            {
                result.Data = VisitanteCryptoHelper.DescifrarVisitante(visitante, _cryptoService);
            }
        }
        catch (Exception e)
        {
            var errorText = $"ERROR: GetVisitanteByCorreoQuery - {e.InnerException?.Message ?? e.Message}";
            result.Errors.Add(errorText);
        }

        return result;
    }
}