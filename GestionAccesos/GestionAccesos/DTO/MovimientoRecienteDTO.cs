namespace GestionAccesos.DTO;

public class MovimientoRecienteDTO
{
    public string NombreTrabajador { get; set; }
    public string TipoMovimiento { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string HaceCuanto => CalcularHaceCuanto(FechaMovimiento);

    private string CalcularHaceCuanto(DateTime fecha)
    {
        var diferencia = DateTime.Now - fecha;
        if (diferencia.TotalMinutes < 60)
            return $"hace {diferencia.TotalMinutes:F0} minutos";
        if (diferencia.TotalHours < 24)
            return $"hace {diferencia.TotalHours:F0} horas";
        return $"hace {diferencia.TotalDays:F0} días";
    }
}