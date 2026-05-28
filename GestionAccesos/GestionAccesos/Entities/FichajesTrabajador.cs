using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("Fichaje")]
public partial class FichajesTrabajador
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdFichaje { get; set; }

    public int? IdTrabajador { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? HoraEntrada { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? HoraSalida { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [ForeignKey("IdTrabajador")]
    [InverseProperty("FichajesTrabajadors")]
    public virtual Trabajadore? IdTrabajadorNavigation { get; set; }

    [InverseProperty("IdFichajeNavigation")]
    public virtual ICollection<PausaFichaje> Pausas { get; set; } = new List<PausaFichaje>();
}
