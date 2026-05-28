using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class AcuerdoFirmadoDTO
{
    public int Id { get; set; }

    public int IdVisitante { get; set; }

    public DateTime? FechaFirma { get; set; }

    public byte[]? Archivo { get; set; }

    public virtual Visitante? IdVisitanteNavigation { get; set; }
}