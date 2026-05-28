using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Mediatr.Fichaje.Query;

namespace GestionAccesos.Services;

public interface IGestionAccesosService
{
    Task<SingleResult<VisitanteDTO>> TratarVisitante(VisitanteDTO visitanteDto);
    Task<ListResult<VisitanteDTO>> GetAllVisitantes();
    Task<SingleResult<VisitanteDTO>> GetVisitanteByCorreo(string correoVisitante);
    Task<SingleResult<AcuerdoFirmadoDTO>> TratarAcuerdo(AcuerdoFirmadoDTO acuerdoDto);
    Task<ListResult<PersonasAvisitarDTO>> GetAllPersonasAVisitar();
    Task<SingleResult<PersonasAvisitarDTO>> TratarPersonaAVisitar(PersonasAvisitarDTO personaDto, bool eliminar = false);
    Task<SingleResult<VisitaDTO>> TratarVisita(VisitaDTO visitaDto);
    Task<ListResult<VisitaDTO>> GetVisitasAbiertas();
    Task<ListResult<VisitaDTO>> GetVisitasHoy();
    Task<ListResult<VisitaDTO>> GetVisitasByDateRange(DateTime startDate, DateTime endDate);
    Task<SingleResult<AcuerdoFirmadoDTO>> GetAcuerdoByIdVisitante(int visitanteId);
    Task<bool> EliminarAcuerdoByIdVisitante(int visitanteId);
    Task<ListResult<TrabajadoresDTO>> GetTrabajadoresDisponiblesPorFechaTemporalidad();
    Task<SingleResult<string>> FicharTrabajador(int trabajadorETTId);
    Task<SingleResult<AusenciasTrabajadorDTO>> GetAusenciaAbiertaByTrabajadorId(int trabajadorId);
    Task<SingleResult<TrabajadoresDTO>> GetTrabajadorETTById(int trabajadorId);
    Task<ListResult<TiposAusenciumDTO>> GetAllTiposAusencia();
    Task<SingleResult<FichajesTrabajadorDTO>> GetFichajeAbiertoByTrabajadorId(int trabajadorId);
    Task<SingleResult<FichajesTrabajadorDTO>> TratarFichaje(FichajesTrabajadorDTO fichajeDto);
    Task<ListResult<FichajesTrabajadorDTO>> GetAllFichajes(DateTime? desde = null, DateTime? hasta = null, bool soloAbiertos = false);
    Task<SingleResult<string>> GestionarAusencia(int trabajadorId, string accion,
        int? tipoAusenciaId = null,
        string? observaciones = null);
    Task<ListResult<EmpresasEttDTO>> GetAllEmpresas();
    Task<SingleResult<EmpresasEttDTO>> TratarEmpresa(EmpresasEttDTO dto, bool eliminar = false);
    Task<ListResult<TrabajadoresDTO>> GetTrabajadoresByDateRange(DateTime startDate, DateTime endDate);
    Task<ListResult<TrabajadoresDTO>> GetAllTrabajadores();
    Task<SingleResult<bool>> DeleteTrabajador(int trabajadorETTId);
    Task<SingleResult<TrabajadoresDTO>> TratarTrabajador(TrabajadoresDTO trabajadorETTDto);
    Task<ListResult<AusenciasTrabajadorDTO>> GetAllAusencias(DateTime? desde = null, DateTime? hasta = null, int? idTrabajador = null, bool soloAbiertas = false);
    Task<SingleResult<TiposAusenciumDTO>> TratarTipoAusencia(TiposAusenciumDTO dto, bool eliminar = false);
    Task<SingleResult<string>> GestionarPausaFichaje(int trabajadorId, string accion);
    Task<SingleResult<EstadoFichajeDTO>> GetEstadoFichajeByTrabajadorId(int trabajadorId);
    Task<ListResult<ContratosEttDTO>> GetAllContratos();
    Task<SingleResult<ContratosEttDTO>> TratarContrato(ContratosEttDTO dto, bool eliminar = false);
    Task<ParametrosWorkerDTO?> GetParametroWorker(string tipo);
    Task SetParametroWorker(string tipo, string valor);
}