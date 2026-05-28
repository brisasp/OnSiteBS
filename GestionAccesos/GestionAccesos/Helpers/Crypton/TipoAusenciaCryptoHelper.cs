using GestionAccesos.DTO;
using GestionAccesos.Entities;
using GestionAccesos.Services;

namespace GestionAccesos.Helpers;

public static class TipoAusenciaCryptoHelper
{
    public static TiposAusenciumDTO DescifrarTipo(TiposAusencium entity, ICryptoService crypto)
    {
        return new TiposAusenciumDTO
        {
            IdTipoAusencia = entity.IdTipoAusencia,
            Descripcion = string.IsNullOrWhiteSpace(entity.Descripcion)
                ? string.Empty
                : crypto.Decrypt(entity.Descripcion),
            Activo = entity.Activo,
            Borrado = entity.Borrado.GetValueOrDefault(),
            FechaRegistro = entity.FechaRegistro,
            FechaBorrado = entity.FechaBorrado
        };
    }

    public static List<TiposAusenciumDTO> DescifrarTipos(
        List<TiposAusencium> entities,
        ICryptoService crypto)
    {
        return entities.Select(e => DescifrarTipo(e, crypto)).ToList();
    }

    public static TiposAusencium CifrarTipoDTO(
        TiposAusenciumDTO dto,
        ICryptoService crypto)
    {
        return new TiposAusencium
        {
            IdTipoAusencia = dto.IdTipoAusencia,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                ? string.Empty
                : crypto.Encrypt(dto.Descripcion),
            Activo = dto.Activo,
            Borrado = dto.Borrado,
            FechaRegistro = dto.FechaRegistro,
            FechaBorrado = dto.FechaBorrado
        };
    }
}