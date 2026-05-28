namespace GestionAccesos.DTO;
public class ParametrosWorkerDTO
{
    public int IdParametro { get; set; }

    public string Tipo { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public int Activo { get; set; }

    public int Borrado { get; set; }

    public string? FechaBorrado { get; set; }

    public string FechaRegistro { get; set; } = null!;

    public string? Unidad { get; set; }
}
