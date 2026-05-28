using GestionAccesos.DTO;
using GestionAccesos.Entities;
using GestionAccesos.Services;

namespace GestionAccesos.Helpers.Crypton;

public static class EmpresaEttCryptoHelper
{
    public static EmpresasEttDTO DescifrarEmpresa(Empresa entity, ICryptoService crypto)
    {
        return new EmpresasEttDTO
        {
            IdEtt = entity.IdEtt,
            Nombre = string.IsNullOrWhiteSpace(entity.Nombre)
                ? string.Empty
                : crypto.Decrypt(entity.Nombre),
            FechaRegistro = entity.FechaRegistro,
            Borrado = entity.Borrado.GetValueOrDefault(),
            FechaBorrado = entity.FechaBorrado
        };
    }

    public static List<EmpresasEttDTO> DescifrarEmpresas(
        List<Empresa> entities,
        ICryptoService crypto)
    {
        return entities
            .Select(e => DescifrarEmpresa(e, crypto))
            .ToList();
    }

    public static Empresa CifrarEmpresaDTO(EmpresasEttDTO dto, ICryptoService crypto)
    {
        return new Empresa
        {
            IdEtt = dto.IdEtt,
            Nombre = string.IsNullOrWhiteSpace(dto.Nombre)
                ? string.Empty
                : crypto.Encrypt(dto.Nombre),
            FechaRegistro = dto.FechaRegistro,
            Borrado = dto.Borrado,
            FechaBorrado = dto.FechaBorrado
        };
    }
}