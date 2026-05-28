using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class EmpresasEttDTO
{
    public int IdEtt { get; set; }

    public string Nombre { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }

    public bool Borrado { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public virtual ICollection<TrabajadoresDTO> Trabajadores { get; set; } = new List<TrabajadoresDTO>();
}