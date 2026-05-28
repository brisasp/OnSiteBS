using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.DTO;
using GestionAccesos.DTO.ResponseModels;
using GestionAccesos.Helpers.Crypton;
using GestionAccesos.Mediatr.Acuerdo.Command;
using GestionAccesos.Mediatr.AcuerdosFirmados.Query;
using GestionAccesos.Mediatr.Ausencia.Command;
using GestionAccesos.Mediatr.Ausencia.Query;
using GestionAccesos.Mediatr.TiposAusencias.Command;
using GestionAccesos.Mediatr.Empresas.Query;
using GestionAccesos.Mediatr.Empresas.Command;
using GestionAccesos.Mediatr.Fichaje.Command;
using GestionAccesos.Mediatr.Fichaje.Query;
using GestionAccesos.Mediatr.PersonasAVisitar.Command;
using GestionAccesos.Mediatr.PersonasAVisitar.Query;
using GestionAccesos.Mediatr.TiposAusencias.Query;
using GestionAccesos.Mediatr.Trabajadores.Command;
using GestionAccesos.Mediatr.Trabajadores.Query;
using GestionAccesos.Mediatr.Contratos.Command;
using GestionAccesos.Mediatr.Contratos.Query;
using GestionAccesos.Mediatr.Visita.Command;
using GestionAccesos.Mediatr.Visitantes.Command;
using GestionAccesos.Mediatr.Visitantes.Query;
using GestionAccesos.Mediatr.Visitas.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Services;

public class GestionAccesosService(AppDbContext appDbContext, IMediator mediator)
    :IGestionAccesosService
{
    public async Task<SingleResult<VisitanteDTO>> TratarVisitante(VisitanteDTO visitanteDto)
    {
        var result = await mediator.Send(new TratarVisitanteCommand(visitanteDto));
        return result;
    }

    public async Task<ListResult<VisitanteDTO>> GetAllVisitantes()
    {
        var model = await mediator.Send(new GetAllVisitantesQuery());
        return model;
    }

    public async Task<SingleResult<VisitanteDTO>> GetVisitanteByCorreo(string correoVisitante)
    {
        var model = await mediator.Send(new GetVisitanteByCorreoQuery(correoVisitante));
        return model;
    }

    public async Task<SingleResult<AcuerdoFirmadoDTO>> TratarAcuerdo(AcuerdoFirmadoDTO acuerdoDto)
    {
        var result = await mediator.Send(new TratarAcuerdoCommand(acuerdoDto));
        return result;
    }

    public async Task<ListResult<PersonasAvisitarDTO>> GetAllPersonasAVisitar()
    {
        var model = await mediator.Send(new GetAllPersonasAVisitarQuery());
        return model;
    }

    public async Task<SingleResult<PersonasAvisitarDTO>> TratarPersonaAVisitar(PersonasAvisitarDTO personaDto, bool eliminar = false)
    {
        var result = await mediator.Send(new TratarPersonaAVisitarCommand(personaDto, eliminar));
        return result;
    }

    public async Task<SingleResult<VisitaDTO>> TratarVisita(VisitaDTO visitaDto)
    {
        var result = await mediator.Send(new TratarVisitaCommand(visitaDto));
        return result;
    }

    public async Task<ListResult<VisitaDTO>> GetVisitasAbiertas()
    {
        var model = await mediator.Send(new GetVisitasAbiertasQuery());
        return model;
    }

    public async Task<ListResult<VisitaDTO>> GetVisitasHoy()
    {
        var model = await mediator.Send(new GetVisitasHoyQuery());
        return model;
    }

    public async Task<ListResult<VisitaDTO>> GetVisitasByDateRange(DateTime startDate, DateTime endDate)
    {
        var model = await mediator.Send(new GetVisitasByDateRangeQuery(startDate, endDate));
        return model;
    }

    public async Task<SingleResult<AcuerdoFirmadoDTO>> GetAcuerdoByIdVisitante(int visitanteId)
    {
        var model = await mediator.Send(new GetAcuerdoByIdVisitanteQuery(visitanteId));
        return model;
    }

    public async Task<bool> EliminarAcuerdoByIdVisitante(int visitanteId)
    {
        var acuerdos = await appDbContext.AcuerdosFirmados
            .Where(a => a.IdVisitante == visitanteId)
            .ToListAsync();

        if (acuerdos.Count == 0) return false;

        appDbContext.AcuerdosFirmados.RemoveRange(acuerdos);
        await appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ListResult<TrabajadoresDTO>> GetTrabajadoresDisponiblesPorFechaTemporalidad()
    {
        var model = await mediator.Send(new GetTrabajadoresDisponiblesPorFechaTemporalidadQuery());
        return model;
    }

    public async Task<SingleResult<string>> FicharTrabajador(int trabajadorETTId)
    {
        return await mediator.Send(new FicharTrabajadorCommand(trabajadorETTId));
    }

    public async Task<SingleResult<AusenciasTrabajadorDTO>> GetAusenciaAbiertaByTrabajadorId(int trabajadorId)
    {
        var model = await mediator.Send(new GetAusenciaAbiertaByTrabajadorIdQuery(trabajadorId));
        return model;
    }
    public async Task<SingleResult<TrabajadoresDTO>> GetTrabajadorETTById(int trabajadorId)
    {
        var model = await mediator.Send(new GetTrabajadorByIdQuery(trabajadorId));
        return model;
    }

    public async Task<ListResult<TiposAusenciumDTO>> GetAllTiposAusencia()
    {
        var model = await mediator.Send(new GetAllTiposAusenciaQuery());
        return model;
    }

    public async Task<SingleResult<FichajesTrabajadorDTO>> GetFichajeAbiertoByTrabajadorId(int trabajadorId)
    {
        var model = await mediator.Send(new GetFichajeAbiertoByTrabajadorIdQuery(trabajadorId));
        return model;
    }

    public async Task<SingleResult<FichajesTrabajadorDTO>> TratarFichaje(FichajesTrabajadorDTO fichajeDto)
    {
        var result = await mediator.Send(new TratarFichajeCommand(fichajeDto, fichajeDto.IdFichaje));
        return result;
    }

    public async Task<ListResult<FichajesTrabajadorDTO>> GetAllFichajes(DateTime? desde = null, DateTime? hasta = null, bool soloAbiertos = false)
    {
        return await mediator.Send(new GetAllFichajesQuery { Desde = desde, Hasta = hasta, SoloAbiertos = soloAbiertos });
    }

    public async Task<SingleResult<string>> GestionarAusencia(int trabajadorId, string accion,
        int? tipoAusenciaId = null,
        string? observaciones = null)
    {
        var command = new GestionarAusenciaCommand(trabajadorId, accion, tipoAusenciaId, observaciones);
        var result = await mediator.Send(command);
        return result;
    }

    public async Task<ListResult<EmpresasEttDTO>> GetAllEmpresas()
    {
        var model = await mediator.Send(new GetAllETTsQuery());
        return model;
    }

    public async Task<SingleResult<EmpresasEttDTO>> TratarEmpresa(EmpresasEttDTO dto, bool eliminar = false)
    {
        var result = await mediator.Send(new TratarEmpresaCommand(dto, eliminar));
        return result;
    }

    public async Task<ListResult<TrabajadoresDTO>> GetTrabajadoresByDateRange(DateTime startDate, DateTime endDate)
    {
        var model = await mediator.Send(new GetTrabajadoresByDateRangeQuery(startDate, endDate));
        return model;
    }

    public async Task<ListResult<TrabajadoresDTO>> GetAllTrabajadores()
    {
        var model = await mediator.Send(new GetAllTrabajadoresQuery());
        return model;
    }

    public async Task<SingleResult<bool>> DeleteTrabajador(int trabajadorETTId)
    {
        var result = await mediator.Send(new DeleteTrabajadorCommand(trabajadorETTId));
        return result;
    }

    public async Task<SingleResult<TrabajadoresDTO>> TratarTrabajador(TrabajadoresDTO trabajadorETTDto)
    {
        var result =
            await mediator.Send(new TratarTrabajadorCommand(trabajadorETTDto, trabajadorETTDto.IdTrabajador));
        return result;
    }

    public async Task<ListResult<AusenciasTrabajadorDTO>> GetAllAusencias(
        DateTime? desde = null, DateTime? hasta = null,
        int? idTrabajador = null, bool soloAbiertas = false)
    {
        return await mediator.Send(new GetAllAusenciasQuery
        {
            Desde = desde,
            Hasta = hasta,
            IdTrabajador = idTrabajador,
            SoloAbiertas = soloAbiertas
        });
    }

    public async Task<SingleResult<TiposAusenciumDTO>> TratarTipoAusencia(TiposAusenciumDTO dto, bool eliminar = false)
    {
        return await mediator.Send(new TratarTipoAusenciaCommand(dto, eliminar));
    }

    public async Task<SingleResult<string>> GestionarPausaFichaje(int trabajadorId, string accion)
    {
        return await mediator.Send(new GestionarPausaFichajeCommand(trabajadorId, accion));
    }

    public async Task<SingleResult<EstadoFichajeDTO>> GetEstadoFichajeByTrabajadorId(int trabajadorId)
    {
        return await mediator.Send(new GetEstadoFichajeByTrabajadorIdQuery { TrabajadorId = trabajadorId });
    }

    public async Task<ListResult<ContratosEttDTO>> GetAllContratos()
    {
        return await mediator.Send(new GetAllContratosQuery());
    }

    public async Task<SingleResult<ContratosEttDTO>> TratarContrato(ContratosEttDTO dto, bool eliminar = false)
    {
        return await mediator.Send(new TratarContratoCommand(dto, eliminar));
    }

    public async Task<ParametrosWorkerDTO?> GetParametroWorker(string tipo)
    {
        var param = await appDbContext.ParametrosWorkers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Tipo == tipo && p.Activo == 1 && p.Borrado == 0);

        if (param == null) return null;

        return new ParametrosWorkerDTO
        {
            IdParametro = param.IdParametro,
            Tipo = param.Tipo,
            Valor = param.Valor,
            Activo = param.Activo,
            Borrado = param.Borrado,
            FechaRegistro = param.FechaRegistro,
            Unidad = param.Unidad
        };
    }

    public async Task SetParametroWorker(string tipo, string valor)
    {
        var param = await appDbContext.ParametrosWorkers
            .FirstOrDefaultAsync(p => p.Tipo == tipo && p.Borrado == 0);

        if (param != null)
        {
            param.Valor = valor;
            param.Activo = 1;
        }
        else
        {
            appDbContext.ParametrosWorkers.Add(new Entities.ParametrosWorker
            {
                Tipo = tipo,
                Valor = valor,
                Activo = 1,
                Borrado = 0,
                FechaRegistro = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
    }
}