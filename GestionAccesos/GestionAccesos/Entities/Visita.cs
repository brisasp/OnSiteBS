using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("Visita")]
public partial class Visita
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdVisita { get; set; }

    public int? IdVisitante { get; set; }

    public int? IdPersona { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaEntrada { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaSalida { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    [ForeignKey("IdPersona")]
    [InverseProperty("Visita")]
    public virtual PersonasAvisitar? IdPersonaNavigation { get; set; }

    [ForeignKey("IdVisitante")]
    [InverseProperty("Visita")]
    public virtual Visitante? IdVisitanteNavigation { get; set; }
}
