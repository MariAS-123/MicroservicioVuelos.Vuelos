using Microservicio.Vuelos.DataAccess.Repositories.Interfaces;
using Microservicio.Vuelos.DataManagement.Interfaces;
using Microservicio.Vuelos.DataManagement.Mappers;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Services;

/// <summary>
/// CAMBIO MICROSERVICIO: DataPagedResult se construye via constructor
/// con parámetros en lugar del inicializador de objeto del monolito.
/// </summary>
public class EscalaDataService : IEscalaDataService
{
    private readonly IEscalaRepository _repo;
    private readonly IUnitOfWork _uow;

    public EscalaDataService(IEscalaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<DataPagedResult<EscalaDataModel>> GetPagedAsync(EscalaFiltroDataModel filtro)
    {
        filtro.PageNumber = filtro.PageNumber <= 0 ? 1 : filtro.PageNumber;
        filtro.PageSize = filtro.PageSize <= 0 ? 10 : filtro.PageSize;

        var data = await _repo.ObtenerTodosAsync();
        var query = data.AsQueryable().Where(x => !x.Eliminado);

        if (filtro.IdVuelo.HasValue)
            query = query.Where(x => x.IdVuelo == filtro.IdVuelo.Value);

        if (filtro.IdAeropuerto.HasValue)
            query = query.Where(x => x.IdAeropuerto == filtro.IdAeropuerto.Value);

        if (filtro.Orden.HasValue)
            query = query.Where(x => x.Orden == filtro.Orden.Value);

        if (!string.IsNullOrWhiteSpace(filtro.TipoEscala))
            query = query.Where(x => x.TipoEscala == filtro.TipoEscala.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(x => x.Estado == filtro.Estado.Trim().ToUpperInvariant());

        if (filtro.FechaLlegadaDesde.HasValue)
            query = query.Where(x => x.FechaHoraLlegada >= filtro.FechaLlegadaDesde.Value);

        if (filtro.FechaLlegadaHasta.HasValue)
            query = query.Where(x => x.FechaHoraLlegada <= filtro.FechaLlegadaHasta.Value);

        if (filtro.FechaSalidaDesde.HasValue)
            query = query.Where(x => x.FechaHoraSalida >= filtro.FechaSalidaDesde.Value);

        if (filtro.FechaSalidaHasta.HasValue)
            query = query.Where(x => x.FechaHoraSalida <= filtro.FechaSalidaHasta.Value);

        query = query
            .OrderBy(x => x.IdVuelo)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.IdEscala);

        var total = query.Count();

        var items = query
            .Skip((filtro.PageNumber - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(EscalaDataMapper.ToDataModel)
            .ToList();

        // CAMBIO: constructor con parámetros en lugar de inicializador de objeto
        return DataPagedResult<EscalaDataModel>.Crear(items, total, filtro.PageNumber, filtro.PageSize);
    }

    public async Task<EscalaDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.ObtenerPorIdAsync(id);
        return entity is null || entity.Eliminado ? null : EscalaDataMapper.ToDataModel(entity);
    }

    public async Task<EscalaDataModel> CreateAsync(EscalaDataModel model)
    {
        var entity = EscalaDataMapper.ToEntity(model);
        entity.Eliminado = false;
        entity.Estado = "ACTIVO";

        await _repo.AgregarAsync(entity);
        await _uow.SaveChangesAsync();

        return EscalaDataMapper.ToDataModel(entity);
    }

    public async Task<EscalaDataModel?> UpdateAsync(EscalaDataModel model)
    {
        var entity = await _repo.ObtenerPorIdParaEditarAsync(model.IdEscala);
        if (entity is null || entity.Eliminado)
            return null;

        EscalaDataMapper.UpdateEntity(entity, model);
        await _uow.SaveChangesAsync();

        return EscalaDataMapper.ToDataModel(entity);
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