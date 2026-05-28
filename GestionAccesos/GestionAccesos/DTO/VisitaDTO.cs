using GestionAccesos.Entities;

namespace GestionAccesos.DTO;

public class VisitaDTO
{
    public int IdVisita { get; set; }

    public int IdVisitante { get; set; }

    public int IdPersona { get; set; }

    public DateTime FechaEntrada { get; set; }

    public DateTime? FechaSalida { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaBorrado { get; set; }

    public bool Borrado { get; set; }

    public string? EmpresaVisitante { get; set; }

    public int? TelefonoVisitante { get; set; }

    public virtual PersonasAvisitarDTO? IdPersonaNavigation { get; set; }

    public virtual VisitanteDTO? IdVisitanteNavigation { get; set; }

    public string NombreCompletoVisitante =>
        $"{IdVisitanteNavigation?.Nombre} {IdVisitanteNavigation?.PrimerApellido}";

    public string NombreCompletoAnfitrion =>
        $"{IdPersonaNavigation?.NombreCompleto}";

    public string Estado
    {
        get
        {
            if (FechaSalida.HasValue)
                return "Checked-out";

            return "Checked-in";
        }
    }
}