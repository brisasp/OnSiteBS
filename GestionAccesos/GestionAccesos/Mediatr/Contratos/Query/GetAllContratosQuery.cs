using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.Contratos.Query;

public class GetAllContratosQuery : IRequest<ListResult<ContratosEttDTO>>
{
}

internal class GetAllContratosQueryHandler
    : IRequestHandler<GetAllContratosQuery, ListResult<ContratosEttDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _crypto;

    public GetAllContratosQueryHandler(AppDbContext context, ICryptoService crypto)
    {
        _context = context;
        _crypto = crypto;
    }

    public async Task<ListResult<ContratosEttDTO>> Handle(
        GetAllContratosQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ListResult<ContratosEttDTO>
        {
            Data = new List<ContratosEttDTO>(),
            Errors = new List<string>()
        };

        try
        {
            var lista = await _context.ContratosTrabajadores
                .AsNoTracking()
                .Include(c => c.IdTrabajadorNavigation)
                .ToListAsync(cancellationToken);

            result.Data = lista.Select(c =>
            {
                var nombre = string.Empty;
                if (c.IdTrabajadorNavigation != null)
                {
                    var n = TryDecrypt(c.IdTrabajadorNavigation.Nombre);
                    var a1 = TryDecrypt(c.IdTrabajadorNavigation.Apellido1);
                    var a2 = TryDecrypt(c.IdTrabajadorNavigation.Apellido2);
                    nombre = $"{n} {a1} {a2}".Trim();
                }

                return new ContratosEttDTO
                {
                    IdContrato = c.IdContrato,
                    IdTrabajador = c.IdTrabajador ?? 0,
                    FechaInicioContrato = c.FechaInicioContrato ?? DateTime.MinValue,
                    FechaFinContrato = c.FechaFinContrato ?? DateTime.MinValue,
                    FechaRegistro = c.FechaRegistro ?? DateTime.Now,
                    FechaBaja = c.FechaBaja,
                    Borrado = c.Borrado ?? false,
                    MotivoBaja = c.MotivoBaja,
                    FechaBorrado = c.FechaBorrado,
                    NombreTrabajador = string.IsNullOrWhiteSpace(nombre) ? "Trabajador desconocido" : nombre
                };
            }).ToList();
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR: GetAllContratos - {e.InnerException?.Message ?? e.Message}");
        }

        return result;
    }

    private string? TryDecrypt(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return valor;
        try { return _crypto.Decrypt(valor); }
        catch { return valor; }
    }
}
