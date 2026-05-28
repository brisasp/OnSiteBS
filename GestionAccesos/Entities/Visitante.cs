using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("Visitante")]
public partial class Visitante
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdVisitante { get; set; }

    public string? Correo { get; set; }

    public byte[]? Foto { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    public string? Nombre { get; set; }

    public string? PrimerApellido { get; set; }

    public string? Empresa { get; set; }

    public int? Telefono { get; set; }

    [InverseProperty("IdVisitanteNavigation")]
    public virtual ICollection<AcuerdosFirmado> AcuerdosFirmados { get; set; } = new List<AcuerdosFirmado>();

    [InverseProperty("IdVisitanteNavigation")]
    public virtual ICollection<Visita> Visita { get; set; } = new List<Visita>();
}
