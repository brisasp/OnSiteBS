using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Entities;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Mediatr.PersonasAVisitar.Command;

public class TratarPersonaAVisitarCommand : IRequest<SingleResult<PersonasAvisitarDTO>>
{
    public PersonasAvisitarDTO PersonaDto { get; set; }
    public bool Eliminar { get; set; }

    public TratarPersonaAVisitarCommand(PersonasAvisitarDTO personaDto, bool eliminar = false)
    {
        PersonaDto = personaDto;
        Eliminar = eliminar;
    }
}

internal class TratarPersonaAVisitarCommandHandler
    : IRequestHandler<TratarPersonaAVisitarCommand, SingleResult<PersonasAvisitarDTO>>
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;

    public TratarPersonaAVisitarCommandHandler(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<SingleResult<PersonasAvisitarDTO>> Handle(
        TratarPersonaAVisitarCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<PersonasAvisitarDTO>
        {
            Data = new PersonasAvisitarDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.Eliminar)
            {
                var persona = await _context.PersonasAvisitars
                    .FirstOrDefaultAsync(p => p.IdPersona == request.PersonaDto.IdPersona, cancellationToken);

                if (persona == null)
                {
                    result.Errors.Add("Persona no encontrada.");
                    return result;
                }

                persona.Borrado = true;
                persona.FechaBorrado = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.PersonaDto;
            }
            else if (request.PersonaDto.IdPersona == 0)
            {
                var nueva = new PersonasAvisitar
                {
                    NombreCompleto = request.PersonaDto.NombreCompleto,
                    Correo = request.PersonaDto.Correo,
                    Departamento = request.PersonaDto.Departamento,
                    Foto = request.PersonaDto.Foto,
                    FechaRegistro = DateTime.Now,
                    Borrado = false
                };

                PersonaAVisitarCryptoHelper.CifrarPersona(nueva, _cryptoService);

                await _context.PersonasAvisitars.AddAsync(nueva, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                request.PersonaDto.IdPersona = nueva.IdPersona;
                request.PersonaDto.FechaRegistro = nueva.FechaRegistro ?? DateTime.Now;
                result.Data = request.PersonaDto;
            }
            else
            {
                var existente = await _context.PersonasAvisitars
                    .FirstOrDefaultAsync(p => p.IdPersona == request.PersonaDto.IdPersona, cancellationToken);

                if (existente == null)
                {
                    result.Errors.Add("Persona no encontrada.");
                    return result;
                }

                existente.NombreCompleto = request.PersonaDto.NombreCompleto;
                existente.Correo = request.PersonaDto.Correo;
                existente.Departamento = request.PersonaDto.Departamento;

                if (request.PersonaDto.Foto != null && request.PersonaDto.Foto.Length > 0)
                    existente.Foto = request.PersonaDto.Foto;

                PersonaAVisitarCryptoHelper.CifrarPersona(existente, _cryptoService);

                await _context.SaveChangesAsync(cancellationToken);

                result.Data = request.PersonaDto;
            }
        }
        catch (Exception e)
        {
            result.Errors.Add($"ERROR al tratar persona: {e.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }
}
