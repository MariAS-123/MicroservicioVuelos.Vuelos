using Microservicio.Vuelos.DataAccess.Repositories.Interfaces;
using Microservicio.Vuelos.DataManagement.Interfaces;
using Microservicio.Vuelos.DataManagement.Mappers;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Services;

/// <summary>
/// CAMBIO MICROSERVICIO: DataPagedResult se construye via constructor
/// con parámetros en lugar del inicializador de objeto del monolito.
/// </summary>
public class AsientoDataService : IAsientoDataService
{
    private readonly IAsientoRepository _repo;
    private readonly IUnitOfWork _uow;

    public AsientoDataService(IAsientoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<DataPagedResult<AsientoDataModel>> GetPagedAsync(AsientoFiltroDataModel filtro)
    {
        filtro.PageNumber = filtro.PageNumber <= 0 ? 1 : filtro.PageNumber;
        filtro.PageSize = filtro.PageSize <= 0 ? 10 : filtro.PageSize;

        var data = await _repo.ObtenerTodosAsync();
        var query = data.AsQueryable().Where(x => !x.Eliminado);

        if (filtro.IdVuelo.HasValue)
            query = query.Where(x => x.IdVuelo == filtro.IdVuelo.Value);

        if (!string.IsNullOrWhiteSpace(filtro.NumeroAsiento))
        {
            var numeroAsiento = filtro.NumeroAsiento.Trim().ToUpperInvariant();
            query = query.Where(x => x.NumeroAsiento.Contains(numeroAsiento));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Clase))
            query = query.Where(x => x.Clase == filtro.Clase.Trim().ToUpperInvariant());

        if (filtro.Disponible.HasValue)
            query = query.Where(x => x.Disponible == filtro.Disponible.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Posicion))
            query = query.Where(x => x.Posicion == filtro.Posicion.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(x => x.Estado == filtro.Estado.Trim().ToUpperInvariant());

        if (filtro.PrecioExtraDesde.HasValue)
            query = query.Where(x => x.PrecioExtra >= filtro.PrecioExtraDesde.Value);

        if (filtro.PrecioExtraHasta.HasValue)
            query = query.Where(x => x.PrecioExtra <= filtro.PrecioExtraHasta.Value);

        query = query
            .OrderBy(x => x.IdVuelo)
            .ThenBy(x => x.NumeroAsiento)
            .ThenBy(x => x.IdAsiento);

        var total = query.Count();

        var items = query
            .Skip((filtro.PageNumber - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(AsientoDataMapper.ToDataModel)
            .ToList();

        // CAMBIO: constructor con parámetros en lugar de inicializador de objeto
        return DataPagedResult<AsientoDataModel>.Crear(items, total, filtro.PageNumber, filtro.PageSize);
    }

    public async Task<AsientoDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.ObtenerPorIdAsync(id);
        return entity is null || entity.Eliminado ? null : AsientoDataMapper.ToDataModel(entity);
    }

    public async Task<IReadOnlyList<AsientoDataModel>> GetByVueloAsync(int idVuelo)
    {
        var data = await _repo.ObtenerTodosAsync();

        return data
            .Where(x => x.IdVuelo == idVuelo && !x.Eliminado)
            .OrderBy(x => x.NumeroAsiento)
            .ThenBy(x => x.IdAsiento)
            .Select(AsientoDataMapper.ToDataModel)
            .ToList();
    }

    public async Task<AsientoDataModel> CreateAsync(AsientoDataModel model)
    {
        var entity = AsientoDataMapper.ToEntity(model);
        entity.Eliminado = false;
        entity.Estado = "ACTIVO";

        await _repo.AgregarAsync(entity);
        await _uow.SaveChangesAsync();

        return AsientoDataMapper.ToDataModel(entity);
    }

    public async Task<AsientoDataModel?> UpdateAsync(AsientoDataModel model)
    {
        var entity = await _repo.ObtenerPorIdParaEditarAsync(model.IdAsiento);
        if (entity is null || entity.Eliminado)
            return null;

        AsientoDataMapper.UpdateEntity(entity, model);
        await _uow.SaveChangesAsync();

        return AsientoDataMapper.ToDataModel(entity);
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