using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAccesos.Entities;

[Table("PausaFichaje")]
public partial class PausaFichaje
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPausa { get; set; }

    public int IdFichaje { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime HoraInicio { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? HoraFin { get; set; }

    public string? Motivo { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime FechaRegistro { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [ForeignKey("IdFichaje")]
    [InverseProperty("Pausas")]
    public virtual FichajesTrabajador? IdFichajeNavigation { get; set; }
}
