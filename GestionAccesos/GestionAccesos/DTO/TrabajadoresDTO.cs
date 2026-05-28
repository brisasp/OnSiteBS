using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class TrabajadoresDTO
{
    public int IdTrabajador { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido1 { get; set; } = null!;

    public string? Apellido2 { get; set; }

    public string Dni { get; set; } = null!;

    public int IdEtt { get; set; }

    public string? Departamento { get; set; }

    public int? TelefonoPersonal { get; set; }

    public string? Observaciones { get; set; }

    public bool Borrado { get; set; }

    public DateTime? FechaMaximaTemporalidad { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<AusenciasTrabajadorDTO> AusenciasTrabajadors { get; set; } = new List<AusenciasTrabajadorDTO>();

    public virtual ICollection<ContratosEttDTO> ContratosEtts { get; set; } = new List<ContratosEttDTO>();

    public virtual ICollection<FichajesTrabajadorDTO> FichajesTrabajadors { get; set; } = new List<FichajesTrabajadorDTO>();

    public virtual EmpresasEttDTO? IdEttNavigation { get; set; }

    public string NombreETT { get; set; } = string.Empty;
}