using Microservicio.Vuelos.DataAccess.Entities;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Mappers;

/// <summary>
/// CAMBIO MICROSERVICIO: eliminada la línea RowVersion = entity.RowVersion
/// en ToDataModel — RowVersion fue removido de Entity y DataModel (era SQL Server).
/// </summary>
public static class VueloDataMapper
{
    public static VueloDataModel ToDataModel(VueloEntity entity) => new()
    {
        IdVuelo = entity.IdVuelo,
        IdAeropuertoOrigen = entity.IdAeropuertoOrigen,
        IdAeropuertoDestino = entity.IdAeropuertoDestino,
        NumeroVuelo = entity.NumeroVuelo,
        FechaHoraSalida = entity.FechaHoraSalida,
        FechaHoraLlegada = entity.FechaHoraLlegada,
        DuracionMin = entity.DuracionMin,
        PrecioBase = entity.PrecioBase,
        CapacidadTotal = entity.CapacidadTotal,
        EstadoVuelo = entity.EstadoVuelo,
        Estado = entity.Estado,
        Eliminado = entity.Eliminado,
        FechaRegistroUtc = entity.FechaRegistroUtc,
        CreadoPorUsuario = entity.CreadoPorUsuario,
        ModificadoPorUsuario = entity.ModificadoPorUsuario,
        FechaModificacionUtc = entity.FechaModificacionUtc,
        ModificacionIp = entity.ModificacionIp
    };

    public static VueloEntity ToEntity(VueloDataModel model) => new()
    {
        IdVuelo = model.IdVuelo,
        IdAeropuertoOrigen = model.IdAeropuertoOrigen,
        IdAeropuertoDestino = model.IdAeropuertoDestino,
        NumeroVuelo = model.NumeroVuelo.Trim().ToUpperInvariant(),
        FechaHoraSalida = model.FechaHoraSalida,
        FechaHoraLlegada = model.FechaHoraLlegada,
        DuracionMin = model.DuracionMin,
        PrecioBase = model.PrecioBase,
        CapacidadTotal = model.CapacidadTotal,
        EstadoVuelo = string.IsNullOrWhiteSpace(model.EstadoVuelo)
            ? "PROGRAMADO"
            : model.EstadoVuelo.Trim().ToUpperInvariant(),
        Estado = string.IsNullOrWhiteSpace(model.Estado)
            ? "ACTIVO"
            : model.Estado.Trim().ToUpperInvariant(),
        Eliminado = model.Eliminado,
        FechaRegistroUtc = model.FechaRegistroUtc == default
            ? DateTime.UtcNow
            : model.FechaRegistroUtc,
        CreadoPorUsuario = model.CreadoPorUsuario.Trim(),
        ModificadoPorUsuario = string.IsNullOrWhiteSpace(model.ModificadoPorUsuario)
            ? null
            : model.ModificadoPorUsuario.Trim(),
        FechaModificacionUtc = model.FechaModificacionUtc,
        ModificacionIp = string.IsNullOrWhiteSpace(model.ModificacionIp)
            ? null
            : model.ModificacionIp.Trim()
    };

    public static void UpdateEntity(VueloEntity entity, VueloDataModel model)
    {
        entity.IdAeropuertoOrigen = model.IdAeropuertoOrigen;
        entity.IdAeropuertoDestino = model.IdAeropuertoDestino;
        entity.NumeroVuelo = model.NumeroVuelo.Trim().ToUpperInvariant();
        entity.FechaHoraSalida = model.FechaHoraSalida;
        entity.FechaHoraLlegada = model.FechaHoraLlegada;
        entity.DuracionMin = model.DuracionMin;
        entity.PrecioBase = model.PrecioBase;
        entity.CapacidadTotal = model.CapacidadTotal;
        entity.EstadoVuelo = model.EstadoVuelo.Trim().ToUpperInvariant();
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