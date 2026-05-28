using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("AcuerdoFirmado")]
public partial class AcuerdosFirmado
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? IdVisitante { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? FechaFirma { get; set; }

    public byte[]? Archivo { get; set; }

    [ForeignKey("IdVisitante")]
    [InverseProperty("AcuerdosFirmados")]
    public virtual Visitante? IdVisitanteNavigation { get; set; }
}
