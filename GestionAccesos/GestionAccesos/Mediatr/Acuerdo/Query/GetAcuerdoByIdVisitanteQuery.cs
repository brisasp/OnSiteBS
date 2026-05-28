using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.AcuerdosFirmados.Query;

public class GetAcuerdoByIdVisitanteQuery : IRequest<SingleResult<AcuerdoFirmadoDTO>>
{
    public int IdVisitante { get; set; }

    public GetAcuerdoByIdVisitanteQuery(int idVisitante)
    {
        IdVisitante = idVisitante;
    }
}

internal class GetAcuerdoByIdVisitanteQueryHandler
    : IRequestHandler<GetAcuerdoByIdVisitanteQuery, SingleResult<AcuerdoFirmadoDTO>>
{
    private readonly AppDbContext _context;

    public GetAcuerdoByIdVisitanteQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<AcuerdoFirmadoDTO>> Handle(
        GetAcuerdoByIdVisitanteQuery request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<AcuerdoFirmadoDTO>
        {
            Data = null,
            Errors = new List<string>()
        };

        try
        {
            var acuerdo = await _context.AcuerdosFirmados
                .AsNoTracking()
                .Where(a => a.IdVisitante == request.IdVisitante)
                .OrderByDescending(a => a.FechaFirma)
                .FirstOrDefaultAsync(cancellationToken);

            if (acuerdo == null)
            {
                result.Errors.Add("No se encontró ningún documento firmado del visitante.");
                return result;
            }

            result.Data = new AcuerdoFirmadoDTO
            {
                Id = acuerdo.Id,
                IdVisitante = acuerdo.IdVisitante ?? 0,
                FechaFirma = acuerdo.FechaFirma,
                Archivo = acuerdo.Archivo
            };
        }
        catch (Exception e)
        {
            result.Errors.Add(
                $"ERROR: GetAcuerdoByIdVisitanteQuery - {(e.InnerException?.Message ?? e.Message)}");
        }

        return result;
    }
}