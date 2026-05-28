using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class TiposAusenciumDTO
{
    public int IdTipoAusencia { get; set; }

    public string Descripcion { get; set; } = null!;

    public bool? Activo { get; set; }

    public bool Borrado { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<AusenciasTrabajadorDTO> AusenciasTrabajadors { get; set; } = new List<AusenciasTrabajadorDTO>();
}