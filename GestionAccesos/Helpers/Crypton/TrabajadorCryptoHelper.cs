using GestionAccesos.DTO;
using GestionAccesos.Entities;
using GestionAccesos.Services;

namespace GestionAccesos.Helpers;

public static class TrabajadorCryptoHelper
{
    public static TrabajadoresDTO DescifrarTrabajador(
        Trabajadore entity,
        ICryptoService crypto)
    {
        return new TrabajadoresDTO
        {
            IdTrabajador = entity.IdTrabajador,

            Nombre = string.IsNullOrWhiteSpace(entity.Nombre)
                ? string.Empty
                : crypto.Decrypt(entity.Nombre),

            Apellido1 = string.IsNullOrWhiteSpace(entity.Apellido1)
                ? string.Empty
                : crypto.Decrypt(entity.Apellido1),

            Apellido2 = string.IsNullOrWhiteSpace(entity.Apellido2)
                ? string.Empty
                : crypto.Decrypt(entity.Apellido2),

            Dni = string.IsNullOrWhiteSpace(entity.Dni)
                ? string.Empty
                : crypto.Decrypt(entity.Dni),

            IdEtt = entity.IdEtt ?? 0,

            Departamento = string.IsNullOrWhiteSpace(entity.Departamento)
                ? string.Empty
                : crypto.Decrypt(entity.Departamento),

            TelefonoPersonal = entity.TelefonoPersonal,

            Observaciones = string.IsNullOrWhiteSpace(entity.Observaciones)
                ? string.Empty
                : crypto.Decrypt(entity.Observaciones),

            FechaRegistro = entity.FechaRegistro,
            FechaBorrado = entity.FechaBorrado,
            FechaMaximaTemporalidad = entity.FechaMaximaTemporalidad,

            Borrado = entity.Borrado.GetValueOrDefault(),

            NombreETT = entity.IdEttNavigation != null &&
                        !string.IsNullOrWhiteSpace(entity.IdEttNavigation.Nombre)
                ? crypto.Decrypt(entity.IdEttNavigation.Nombre)
                : string.Empty
        };
    }

    public static List<TrabajadoresDTO> DescifrarTrabajadores(
        List<Trabajadore> entities,
        ICryptoService crypto)
    {
        return entities
            .Select(e => DescifrarTrabajador(e, crypto))
            .ToList();
    }

    public static Trabajadore CifrarTrabajadorDTO(
        TrabajadoresDTO dto,
        ICryptoService crypto)
    {
        return new Trabajadore
        {
            IdTrabajador = dto.IdTrabajador,

            Nombre = string.IsNullOrWhiteSpace(dto.Nombre)
                ? string.Empty
                : crypto.Encrypt(dto.Nombre),

            Apellido1 = string.IsNullOrWhiteSpace(dto.Apellido1)
                ? string.Empty
                : crypto.Encrypt(dto.Apellido1),

            Apellido2 = string.IsNullOrWhiteSpace(dto.Apellido2)
                ? string.Empty
                : crypto.Encrypt(dto.Apellido2),

            Dni = string.IsNullOrWhiteSpace(dto.Dni)
                ? string.Empty
                : crypto.Encrypt(dto.Dni),

            IdEtt = dto.IdEtt,

            Departamento = string.IsNullOrWhiteSpace(dto.Departamento)
                ? string.Empty
                : crypto.Encrypt(dto.Departamento),

            TelefonoPersonal = dto.TelefonoPersonal,

            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones)
                ? string.Empty
                : crypto.Encrypt(dto.Observaciones),

            FechaRegistro = dto.FechaRegistro,
            FechaBorrado = dto.FechaBorrado,
            FechaMaximaTemporalidad = dto.FechaMaximaTemporalidad,

            Borrado = dto.Borrado
        };
    }
}