using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("PersonaAVisitar")]
public partial class PersonasAvisitar
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPersona { get; set; }

    public string? NombreCompleto { get; set; }

    public string? Correo { get; set; }

    public string? Departamento { get; set; }

    public byte[]? Foto { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    [InverseProperty("IdPersonaNavigation")]
    public virtual ICollection<Visita> Visita { get; set; } = new List<Visita>();
}
