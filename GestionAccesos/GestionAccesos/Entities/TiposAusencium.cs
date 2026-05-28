using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("TipoAusencia")]
public partial class TiposAusencium
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTipoAusencia { get; set; }

    public string? Descripcion { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Activo { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("MotivoNavigation")]
    public virtual ICollection<AusenciasTrabajador> AusenciasTrabajadors { get; set; } = new List<AusenciasTrabajador>();
}
