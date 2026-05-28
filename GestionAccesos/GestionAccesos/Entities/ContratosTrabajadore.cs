using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("ContratoTrabajador")]
public partial class ContratosTrabajadore
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdContrato { get; set; }

    public int? IdTrabajador { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaInicioContrato { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaFinContrato { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBaja { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    public string? MotivoBaja { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [ForeignKey("IdTrabajador")]
    [InverseProperty("ContratosTrabajadores")]
    public virtual Trabajadore? IdTrabajadorNavigation { get; set; }
}
