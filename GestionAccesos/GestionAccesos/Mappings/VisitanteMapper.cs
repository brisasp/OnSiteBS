using GestionAccesos.DTO;
using GestionAccesos.Entities;

namespace GestionAccesos.Mappings;
public static class VisitanteMapper
{
    public static Visitante ToEntity(VisitanteDTO dto)
    {
        return new Visitante
        {
            IdVisitante = dto.IdVisitante,
            Nombre = dto.Nombre,
            PrimerApellido = dto.PrimerApellido,
            Empresa = dto.Empresa,
            Correo = dto.Correo,
            Telefono = dto.Telefono,
            Foto = dto.Foto
        };
    }

    public static void UpdateEntity(Visitante entity, VisitanteDTO dto)
    {
        entity.Nombre = dto.Nombre;
        entity.PrimerApellido = dto.PrimerApellido;
        entity.Empresa = dto.Empresa;
        entity.Correo = dto.Correo;
        entity.Telefono = dto.Telefono;

        if (dto.Foto != null)
            entity.Foto = dto.Foto;
    }
}