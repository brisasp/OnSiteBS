using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class VisitanteDTO
{
    public int IdVisitante { get; set; }

    public string? Correo { get; set; }

    public byte[]? Foto { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public bool Borrado { get; set; }

    public string Nombre { get; set; } = null!;

    public string PrimerApellido { get; set; } = null!;

    public string? Empresa { get; set; }

    public int? Telefono { get; set; }

    public virtual ICollection<AcuerdoFirmadoDTO> AcuerdosFirmados { get; set; } = new List<AcuerdoFirmadoDTO>();

    public virtual ICollection<VisitaDTO> Visita { get; set; } = new List<VisitaDTO>();

    public string NombreCompleto => $"{Nombre} {PrimerApellido}";
}