using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Mappings;
using MediatR;

namespace GestionAccesos.Mediatr.Acuerdo.Command;

public class TratarAcuerdoCommand : IRequest<SingleResult<AcuerdoFirmadoDTO>>
{
    public AcuerdoFirmadoDTO AcuerdoFirmadoDto { get; set; }

    public TratarAcuerdoCommand(AcuerdoFirmadoDTO acuerdoFirmadoDto)
    {
        AcuerdoFirmadoDto = acuerdoFirmadoDto;
    }
}

internal class TratarAcuerdoCommandHandler
    : IRequestHandler<TratarAcuerdoCommand, SingleResult<AcuerdoFirmadoDTO>>
{
    private readonly AppDbContext _context;

    public TratarAcuerdoCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SingleResult<AcuerdoFirmadoDTO>> Handle(
        TratarAcuerdoCommand request,
        CancellationToken cancellationToken)
    {
        var result = new SingleResult<AcuerdoFirmadoDTO>
        {
            Data = new AcuerdoFirmadoDTO(),
            Errors = new List<string>()
        };

        try
        {
            if (request.AcuerdoFirmadoDto == null)
            {
                result.Errors.Add("No se ha recibido el acuerdo.");
                return result;
            }

            if (request.AcuerdoFirmadoDto.Archivo == null || request.AcuerdoFirmadoDto.Archivo.Length == 0)
            {
                result.Errors.Add("El archivo del acuerdo está vacío.");
                return result;
            }

            var nuevoAcuerdo = AcuerdoMapper.ToEntity(request.AcuerdoFirmadoDto);

            if (nuevoAcuerdo.Archivo == null || nuevoAcuerdo.Archivo.Length == 0)
            {
                result.Errors.Add("El archivo del acuerdo no se ha podido mapear correctamente.");
                return result;
            }

            await _context.AcuerdosFirmados.AddAsync(nuevoAcuerdo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            result.Data = AcuerdoMapper.ToDto(nuevoAcuerdo);
        }
        catch (Exception e)
        {
            result.Errors.Add($"Error al registrar el acuerdo del visitante: {e.Message}");
        }

        _context.ChangeTracker.Clear();
        return result;
    }
}