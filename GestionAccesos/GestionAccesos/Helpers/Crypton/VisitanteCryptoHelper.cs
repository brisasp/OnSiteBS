using GestionAccesos.DTO;
using GestionAccesos.Entities;
using GestionAccesos.Services;

namespace GestionAccesos.Helpers.Crypton;

public static class VisitanteCryptoHelper
{
    public static VisitanteDTO DescifrarVisitante(Visitante entity, ICryptoService crypto)
    {
        return new VisitanteDTO
        {
            IdVisitante = entity.IdVisitante,
            Correo = string.IsNullOrWhiteSpace(entity.Correo) ? null : crypto.Decrypt(entity.Correo),
            Telefono = entity.Telefono,
            Nombre = string.IsNullOrWhiteSpace(entity.Nombre) ? string.Empty : crypto.Decrypt(entity.Nombre),
            PrimerApellido = string.IsNullOrWhiteSpace(entity.PrimerApellido) ? string.Empty : crypto.Decrypt(entity.PrimerApellido),
            Empresa = string.IsNullOrWhiteSpace(entity.Empresa) ? null : crypto.Decrypt(entity.Empresa),
            Foto = entity.Foto != null && entity.Foto.Length > 0 ? crypto.DecryptBytes(entity.Foto) : null,
            FechaRegistro = entity.FechaRegistro ?? DateTime.Now,
            FechaBorrado = entity.FechaBorrado,
            Borrado = entity.Borrado.GetValueOrDefault()
        };
    }

    public static List<VisitanteDTO> DescifrarVisitantes(List<Visitante> entities, ICryptoService crypto)
    {
        return entities.Select(e => DescifrarVisitante(e, crypto)).ToList();
    }

    public static void CifrarVisitante(Visitante entity, ICryptoService crypto)
    {
        entity.Correo = string.IsNullOrWhiteSpace(entity.Correo) ? entity.Correo : crypto.Encrypt(entity.Correo);
        entity.Nombre = string.IsNullOrWhiteSpace(entity.Nombre) ? entity.Nombre : crypto.Encrypt(entity.Nombre);
        entity.PrimerApellido = string.IsNullOrWhiteSpace(entity.PrimerApellido) ? entity.PrimerApellido : crypto.Encrypt(entity.PrimerApellido);
        entity.Empresa = string.IsNullOrWhiteSpace(entity.Empresa) ? entity.Empresa : crypto.Encrypt(entity.Empresa);

        if (entity.Foto != null && entity.Foto.Length > 0)
            entity.Foto = crypto.EncryptBytes(entity.Foto);
    }
}