using Microservicio.Vuelos.DataAccess.Entities;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Mappers;

/// <summary>
/// CAMBIO MICROSERVICIO: eliminada la línea RowVersion = entity.RowVersion
/// en ToDataModel — RowVersion fue removido de Entity y DataModel (era SQL Server).
/// </summary>
public static class AsientoDataMapper
{
    public static AsientoDataModel ToDataModel(AsientoEntity entity) => new()
    {
        IdAsiento = entity.IdAsiento,
        IdVuelo = entity.IdVuelo,
        NumeroAsiento = entity.NumeroAsiento,
        Clase = entity.Clase,
        Disponible = entity.Disponible,
        PrecioExtra = entity.PrecioExtra,
        Posicion = entity.Posicion,
        Estado = entity.Estado,
        Eliminado = entity.Eliminado,
        FechaRegistroUtc = entity.FechaRegistroUtc,
        CreadoPorUsuario = entity.CreadoPorUsuario,
        ModificadoPorUsuario = entity.ModificadoPorUsuario,
        FechaModificacionUtc = entity.FechaModificacionUtc,
        ModificacionIp = entity.ModificacionIp
    };

    public static AsientoEntity ToEntity(AsientoDataModel model) => new()
    {
        IdAsiento = model.IdAsiento,
        IdVuelo = model.IdVuelo,
        NumeroAsiento = model.NumeroAsiento.Trim().ToUpperInvariant(),
        Clase = model.Clase.Trim().ToUpperInvariant(),
        Disponible = model.Disponible,
        PrecioExtra = model.PrecioExtra,
        Posicion = string.IsNullOrWhiteSpace(model.Posicion)
            ? null
            : model.Posicion.Trim().ToUpperInvariant(),
        Estado = string.IsNullOrWhiteSpace(model.Estado)
            ? "ACTIVO"
            : model.Estado.Trim().ToUpperInvariant(),
        Eliminado = model.Eliminado,
        FechaRegistroUtc = model.FechaRegistroUtc,
        CreadoPorUsuario = model.CreadoPorUsuario.Trim(),
        ModificadoPorUsuario = string.IsNullOrWhiteSpace(model.ModificadoPorUsuario)
            ? null
            : model.ModificadoPorUsuario.Trim(),
        FechaModificacionUtc = model.FechaModificacionUtc,
        ModificacionIp = string.IsNullOrWhiteSpace(model.ModificacionIp)
            ? null
            : model.ModificacionIp.Trim()
    };

    public static void UpdateEntity(AsientoEntity entity, AsientoDataModel model)
    {
        entity.IdVuelo = model.IdVuelo;
        entity.NumeroAsiento = model.NumeroAsiento.Trim().ToUpperInvariant();
        entity.Clase = model.Clase.Trim().ToUpperInvariant();
        entity.Disponible = model.Disponible;
        entity.PrecioExtra = model.PrecioExtra;
        entity.Posicion = string.IsNullOrWhiteSpace(model.Posicion)
            ? null
            : model.Posicion.Trim().ToUpperInvariant();
        entity.Estado = string.IsNullOrWhiteSpace(model.Estado)
            ? entity.Estado
            : model.Estado.Trim().ToUpperInvariant();
        entity.ModificadoPorUsuario = string.IsNullOrWhiteSpace(model.ModificadoPorUsuario)
            ? null
            : model.ModificadoPorUsuario.Trim();
        entity.FechaModificacionUtc = model.FechaModificacionUtc;
        entity.ModificacionIp = string.IsNullOrWhiteSpace(model.ModificacionIp)
            ? null
            : model.ModificacionIp.Trim();
    }
}