using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class ContratosEttDTO
{
    public int IdContrato { get; set; }

    public int IdTrabajador { get; set; }

    public DateTime FechaInicioContrato { get; set; }

    public DateTime FechaFinContrato { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaBaja { get; set; }

    public bool Borrado { get; set; }

    public string? MotivoBaja { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public virtual TrabajadoresDTO? IdTrabajadorNavigation { get; set; }

    public string NombreTrabajador { get; set; } = string.Empty;
}