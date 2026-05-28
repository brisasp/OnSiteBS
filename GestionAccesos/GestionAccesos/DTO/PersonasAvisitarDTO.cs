using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class PersonasAvisitarDTO
{
    public int IdPersona { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string? Correo { get; set; }

    public string? Departamento { get; set; }

    public byte[]? Foto { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public bool Borrado { get; set; }

    public virtual ICollection<VisitaDTO> Visita { get; set; } = new List<VisitaDTO>();
}