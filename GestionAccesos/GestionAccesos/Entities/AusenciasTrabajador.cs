using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("AusenciaTrabajador")]
public partial class AusenciasTrabajador
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdAusencia { get; set; }

    public int? IdTrabajador { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? HoraInicio { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? HoraFin { get; set; }

    public int? Motivo { get; set; }

    public string? Observaciones { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [ForeignKey("IdTrabajador")]
    [InverseProperty("AusenciasTrabajadors")]
    public virtual Trabajadore? IdTrabajadorNavigation { get; set; }

    [ForeignKey("Motivo")]
    [InverseProperty("AusenciasTrabajadors")]
    public virtual TiposAusencium? MotivoNavigation { get; set; }
}
