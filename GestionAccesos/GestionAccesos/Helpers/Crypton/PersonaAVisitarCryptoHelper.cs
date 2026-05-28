using GestionAccesos.DTO;
using GestionAccesos.Entities;
using GestionAccesos.Services;

namespace GestionAccesos.Helpers.Crypton;

public static class PersonaAVisitarCryptoHelper
{
    public static PersonasAvisitarDTO DescifrarPersona(PersonasAvisitar entity, ICryptoService crypto)
    {
        return new PersonasAvisitarDTO
        {
            IdPersona = entity.IdPersona,
            NombreCompleto = string.IsNullOrWhiteSpace(entity.NombreCompleto)
        ? string.Empty
        : crypto.Decrypt(entity.NombreCompleto),
            Correo = string.IsNullOrWhiteSpace(entity.Correo)
        ? null
        : crypto.Decrypt(entity.Correo),
            Departamento = string.IsNullOrWhiteSpace(entity.Departamento)
        ? null
        : crypto.Decrypt(entity.Departamento),
            Foto = entity.Foto != null && entity.Foto.Length > 0
        ? crypto.DecryptBytes(entity.Foto)
        : null,
            FechaRegistro = entity.FechaRegistro ?? DateTime.Now,
            FechaBorrado = entity.FechaBorrado,
            Borrado = entity.Borrado.GetValueOrDefault()
        };
    }

    public static List<PersonasAvisitarDTO> DescifrarPersonas(List<PersonasAvisitar> entities, ICryptoService crypto)
    {
        return entities.Select(e => DescifrarPersona(e, crypto)).ToList();
    }

    public static void CifrarPersona(PersonasAvisitar entity, ICryptoService crypto)
    {
        entity.NombreCompleto = string.IsNullOrWhiteSpace(entity.NombreCompleto)
            ? entity.NombreCompleto
            : crypto.Encrypt(entity.NombreCompleto);

        entity.Correo = string.IsNullOrWhiteSpace(entity.Correo)
            ? entity.Correo
            : crypto.Encrypt(entity.Correo);

        entity.Departamento = string.IsNullOrWhiteSpace(entity.Departamento)
            ? entity.Departamento
            : crypto.Encrypt(entity.Departamento);

        if (entity.Foto != null && entity.Foto.Length > 0)
            entity.Foto = crypto.EncryptBytes(entity.Foto);
    }
}