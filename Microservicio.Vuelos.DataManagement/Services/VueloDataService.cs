using Microservicio.Vuelos.DataAccess.Repositories.Interfaces;
using Microservicio.Vuelos.DataManagement.Interfaces;
using Microservicio.Vuelos.DataManagement.Mappers;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Services;

/// <summary>
/// CAMBIO MICROSERVICIO: DataPagedResult se construye via constructor
/// con parámetros (items, totalRegistros, paginaActual, tamanoPagina)
/// en lugar del inicializador de objeto del monolito.
/// </summary>
public class VueloDataService : IVueloDataService
{
    private readonly IVueloRepository _repo;
    private readonly IUnitOfWork _uow;

    public VueloDataService(IVueloRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<DataPagedResult<VueloDataModel>> GetPagedAsync(VueloFiltroDataModel filtro)
    {
        filtro.PageNumber = filtro.PageNumber <= 0 ? 1 : filtro.PageNumber;
        filtro.PageSize = filtro.PageSize <= 0 ? 10 : filtro.PageSize;

        var data = await _repo.ObtenerTodosAsync();
        var query = data.AsQueryable();

        if (!filtro.IncluirEliminados)
            query = query.Where(x => !x.Eliminado);

        if (!string.IsNullOrWhiteSpace(filtro.NumeroVuelo))
        {
            var numeroVuelo = filtro.NumeroVuelo.Trim().ToUpperInvariant();
            query = query.Where(x => x.NumeroVuelo.Contains(numeroVuelo));
        }

        if (filtro.IdAeropuertoOrigen.HasValue)
            query = query.Where(x => x.IdAeropuertoOrigen == filtro.IdAeropuertoOrigen.Value);

        if (filtro.IdAeropuertoDestino.HasValue)
            query = query.Where(x => x.IdAeropuertoDestino == filtro.IdAeropuertoDestino.Value);

        if (!string.IsNullOrWhiteSpace(filtro.EstadoVuelo))
            query = query.Where(x => x.EstadoVuelo == filtro.EstadoVuelo.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(x => x.Estado == filtro.Estado.Trim().ToUpperInvariant());

        if (filtro.FechaSalidaDesde.HasValue)
            query = query.Where(x => x.FechaHoraSalida >= filtro.FechaSalidaDesde.Value);

        if (filtro.FechaSalidaHasta.HasValue)
            query = query.Where(x => x.FechaHoraSalida <= filtro.FechaSalidaHasta.Value);

        query = query
            .OrderBy(x => x.FechaHoraSalida)
            .ThenBy(x => x.NumeroVuelo)
            .ThenBy(x => x.IdVuelo);

        var total = query.Count();

        var items = query
            .Skip((filtro.PageNumber - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(VueloDataMapper.ToDataModel)
            .ToList();

        // CAMBIO: constructor con parámetros en lugar de inicializador de objeto
        return DataPagedResult<VueloDataModel>.Crear(items, total, filtro.PageNumber, filtro.PageSize);
    }

    public async Task<VueloDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.ObtenerPorIdAsync(id);
        if (entity is null || entity.Eliminado)
            return null;

        return VueloDataMapper.ToDataModel(entity);
    }

    public async Task<VueloDataModel> CreateAsync(VueloDataModel model)
    {
        var entity = VueloDataMapper.ToEntity(model);
        entity.Eliminado = false;
        entity.Estado = "ACTIVO";

        if (string.IsNullOrWhiteSpace(entity.EstadoVuelo))
            entity.EstadoVuelo = "PROGRAMADO";

        await _repo.AgregarAsync(entity);
        await _uow.SaveChangesAsync();

        return VueloDataMapper.ToDataModel(entity);
    }

    public async Task<VueloDataModel?> UpdateAsync(VueloDataModel model)
    {
        var entity = await _repo.ObtenerPorIdParaEditarAsync(model.IdVuelo);
        if (entity is null || entity.Eliminado)
            return null;

        VueloDataMapper.UpdateEntity(entity, model);
        entity.FechaModificacionUtc = DateTime.UtcNow;

        await _uow.SaveChangesAsync();

        return VueloDataMapper.ToDataModel(entity);
    }

    public async Task<bool> DeleteAsync(int id, string modificadoPorUsuario)
    {
        var entity = await _repo.ObtenerPorIdParaEditarAsync(id);
        if (entity is null || entity.Eliminado)
            return false;

        entity.Eliminado = true;
        entity.Estado = "INACTIVO";
        entity.ModificadoPorUsuario = modificadoPorUsuario.Trim();
        entity.FechaModificacionUtc = DateTime.UtcNow;

        await _uow.SaveChangesAsync();
        return true;
    }
}