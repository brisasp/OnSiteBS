using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("Trabajador")]
public partial class Trabajadore
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTrabajador { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido1 { get; set; }

    public string? Apellido2 { get; set; }

    [Column("DNI")]
    public string? Dni { get; set; }

    [Column("IdEmpresa")]
    public int? IdEtt { get; set; }

    public string? Departamento { get; set; }

    public int? TelefonoPersonal { get; set; }

    public string? Observaciones { get; set; }

    [Column(TypeName = "BOOLEAN")]
    public bool? Borrado { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaMaximaTemporalidad { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaBorrado { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("IdTrabajadorNavigation")]
    public virtual ICollection<AusenciasTrabajador> AusenciasTrabajadors { get; set; } = new List<AusenciasTrabajador>();

    [InverseProperty("IdTrabajadorNavigation")]
    public virtual ICollection<ContratosTrabajadore> ContratosTrabajadores { get; set; } = new List<ContratosTrabajadore>();

    [InverseProperty("IdTrabajadorNavigation")]
    public virtual ICollection<FichajesTrabajador> FichajesTrabajadors { get; set; } = new List<FichajesTrabajador>();

    [ForeignKey("IdEtt")]
    [InverseProperty("Trabajadores")]
    public virtual Empresa? IdEttNavigation { get; set; }
}
