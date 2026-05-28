using GestionAccesos.DTO;
using GestionAccesos.Entities;

namespace GestionAccesos.Mappings;

public class AcuerdoMapper
{
    public static AcuerdosFirmado ToEntity(AcuerdoFirmadoDTO dto)
    {
        return new AcuerdosFirmado
        {
            Id = dto.Id,
            IdVisitante = dto.IdVisitante,
            FechaFirma = dto.FechaFirma,
            Archivo = dto.Archivo,
            IdVisitanteNavigation = dto.IdVisitanteNavigation
        };
    }

    public static void UpdateEntity(AcuerdosFirmado entity, AcuerdoFirmadoDTO dto)
    {
        entity.IdVisitante = dto.IdVisitante;
        entity.FechaFirma = dto.FechaFirma;
        entity.Archivo = dto.Archivo;

        if (dto.IdVisitanteNavigation != null)
            entity.IdVisitanteNavigation = dto.IdVisitanteNavigation;
    }

    public static AcuerdoFirmadoDTO ToDto(AcuerdosFirmado entity)
    {
        return new AcuerdoFirmadoDTO
        {
            Id = entity.Id,
            IdVisitante = entity.IdVisitante ?? 0,
            FechaFirma = entity.FechaFirma,
            Archivo = entity.Archivo,
            IdVisitanteNavigation = entity.IdVisitanteNavigation
        };
    }
}