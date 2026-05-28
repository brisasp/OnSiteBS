using GestionAccesos.DTO;

namespace GestionAccesos.Services.ExcelExporter;

public class ExcelExportCoordinator
{
    //private readonly FichajesExcelExporter _fichajesExporter;
    //private readonly AusenciasExcelExporter _ausenciasExporter;
    //private readonly TrabajadoresExcelExporter _trabajadoresExporter;
    private readonly VisitasExcelExporter _visitasExporter;

    public ExcelExportCoordinator(
        //FichajesExcelExporter fichajesExporter,
        //AusenciasExcelExporter ausenciasExporter,
        //TrabajadoresExcelExporter trabajadoresExporter,
        VisitasExcelExporter visitasExporter)
    {
        //_fichajesExporter = fichajesExporter;
        //_ausenciasExporter = ausenciasExporter;
        //_trabajadoresExporter = trabajadoresExporter;
        _visitasExporter = visitasExporter;
    }

    //public async Task ExportarReporteFichajes(
    //    List<FichajesTrabajadorDTO> fichajes,
    //    List<TrabajadoresEttDTO> trabajadores,
    //    List<EmpresasEttDTO> etts,
    //    DateTime fechaInicio,
    //    DateTime fechaFin)
    //{
    //    await _fichajesExporter.ExportarReporteCompletoFichajes(fichajes, trabajadores, etts, fechaInicio, fechaFin);
    //}

    //public async Task ExportarReporteAusencias(
    //    List<AusenciasTrabajadorDTO> ausencias,
    //    List<TrabajadoresEttDTO> trabajadores,
    //    List<EmpresasEttDTO> etts,
    //    DateTime fechaInicio,
    //    DateTime fechaFin)
    //{
    //    await _ausenciasExporter.ExportarReporteCompletoAusencias(ausencias, trabajadores, etts, fechaInicio, fechaFin);
    //}

    //public async Task ExportarTrabajadores(
    //    List<TrabajadoresEttDTO> trabajadores,
    //    List<EmpresasEttDTO> etts)
    //{
    //    await _trabajadoresExporter.Exportar(trabajadores, etts);
    //}

    public async Task ExportarVisitas(List<VisitaDTO> visitas)
    {
        await _visitasExporter.Exportar(visitas);
    }

    public async Task ExportarFichajes(List<FichajesTrabajadorDTO> fichajes)
    {
        await _visitasExporter.ExportarFichajes(fichajes);
    }
}