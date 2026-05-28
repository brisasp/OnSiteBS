using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Entities;

[Table("ParametrosWorker")]
public partial class ParametrosWorker
{
    [Key]
    public int IdParametro { get; set; }

    public string Tipo { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public int Activo { get; set; }

    public int Borrado { get; set; }

    public string? FechaBorrado { get; set; }

    public string FechaRegistro { get; set; } = null!;

    public string? Unidad { get; set; }
}
