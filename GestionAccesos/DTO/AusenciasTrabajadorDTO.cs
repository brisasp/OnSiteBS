using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class AusenciasTrabajadorDTO
{
    public int IdAusencia { get; set; }

    public int IdTrabajador { get; set; }

    public DateTime HoraInicio { get; set; }

    public DateTime? HoraFin { get; set; }

    public int Motivo { get; set; }

    public string? Observaciones { get; set; }

    public bool Borrado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public virtual TrabajadoresDTO? IdTrabajadorNavigation { get; set; }

    public virtual TiposAusenciumDTO? MotivoNavigation { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string DescripcionMotivo { get; set; } = string.Empty;
}