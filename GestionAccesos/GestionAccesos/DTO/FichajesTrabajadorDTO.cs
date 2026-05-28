using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class FichajesTrabajadorDTO
{
    public int IdFichaje { get; set; }

    public int IdTrabajador { get; set; }

    public DateTime? HoraEntrada { get; set; } = null!;

    public DateTime? HoraSalida { get; set; }

    public bool Borrado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public virtual TrabajadoresDTO? IdTrabajadorNavigation { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Departamento { get; set; }
    public string? Observaciones { get; set; }
}