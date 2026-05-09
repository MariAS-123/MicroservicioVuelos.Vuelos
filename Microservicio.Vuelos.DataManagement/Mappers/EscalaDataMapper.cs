using Microservicio.Vuelos.DataAccess.Entities;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Mappers;

/// <summary>
/// CAMBIO MICROSERVICIO: eliminada la línea RowVersion = entity.RowVersion
/// en ToDataModel — RowVersion fue removido de Entity y DataModel (era SQL Server).
/// </summary>
public static class EscalaDataMapper
{
    public static EscalaDataModel ToDataModel(EscalaEntity entity) => new()
    {
        IdEscala = entity.IdEscala,
        IdVuelo = entity.IdVuelo,
        IdAeropuerto = entity.IdAeropuerto,
        Orden = entity.Orden,
        FechaHoraLlegada = entity.FechaHoraLlegada,
        FechaHoraSalida = entity.FechaHoraSalida,
        DuracionMin = entity.DuracionMin,
        TipoEscala = entity.TipoEscala,
        Terminal = entity.Terminal,
        Puerta = entity.Puerta,
        Observaciones = entity.Observaciones,
        Estado = entity.Estado,
        Eliminado = entity.Eliminado,
        FechaRegistroUtc = entity.FechaRegistroUtc,
        CreadoPorUsuario = entity.CreadoPorUsuario,
        ModificadoPorUsuario = entity.ModificadoPorUsuario,
        FechaModificacionUtc = entity.FechaModificacionUtc,
        ModificacionIp = entity.ModificacionIp
    };

    public static EscalaEntity ToEntity(EscalaDataModel model) => new()
    {
        IdEscala = model.IdEscala,
        IdVuelo = model.IdVuelo,
        IdAeropuerto = model.IdAeropuerto,
        Orden = model.Orden,
        FechaHoraLlegada = model.FechaHoraLlegada,
        FechaHoraSalida = model.FechaHoraSalida,
        DuracionMin = model.DuracionMin,
        TipoEscala = model.TipoEscala.Trim().ToUpperInvariant(),
        Terminal = string.IsNullOrWhiteSpace(model.Terminal) ? null : model.Terminal.Trim(),
        Puerta = string.IsNullOrWhiteSpace(model.Puerta) ? null : model.Puerta.Trim(),
        Observaciones = string.IsNullOrWhiteSpace(model.Observaciones) ? null : model.Observaciones.Trim(),
        Estado = string.IsNullOrWhiteSpace(model.Estado)
            ? "ACTIVO"
            : model.Estado.Trim().ToUpperInvariant(),
        Eliminado = model.Eliminado,
        FechaRegistroUtc = model.FechaRegistroUtc,
        CreadoPorUsuario = model.CreadoPorUsuario.Trim(),
        ModificadoPorUsuario = string.IsNullOrWhiteSpace(model.ModificadoPorUsuario) ? null : model.ModificadoPorUsuario.Trim(),
        FechaModificacionUtc = model.FechaModificacionUtc,
        ModificacionIp = string.IsNullOrWhiteSpace(model.ModificacionIp) ? null : model.ModificacionIp.Trim()
    };

    public static void UpdateEntity(EscalaEntity entity, EscalaDataModel model)
    {
        entity.IdVuelo = model.IdVuelo;
        entity.IdAeropuerto = model.IdAeropuerto;
        entity.Orden = model.Orden;
        entity.FechaHoraLlegada = model.FechaHoraLlegada;
        entity.FechaHoraSalida = model.FechaHoraSalida;
        entity.DuracionMin = model.DuracionMin;
        entity.TipoEscala = model.TipoEscala.Trim().ToUpperInvariant();
        entity.Terminal = string.IsNullOrWhiteSpace(model.Terminal) ? null : model.Terminal.Trim();
        entity.Puerta = string.IsNullOrWhiteSpace(model.Puerta) ? null : model.Puerta.Trim();
        entity.Observaciones = string.IsNullOrWhiteSpace(model.Observaciones) ? null : model.Observaciones.Trim();
        entity.Estado = string.IsNullOrWhiteSpace(model.Estado)
            ? entity.Estado
            : model.Estado.Trim().ToUpperInvariant();
        entity.ModificadoPorUsuario = string.IsNullOrWhiteSpace(model.ModificadoPorUsuario) ? null : model.ModificadoPorUsuario.Trim();
        entity.FechaModificacionUtc = model.FechaModificacionUtc;
        entity.ModificacionIp = string.IsNullOrWhiteSpace(model.ModificacionIp) ? null : model.ModificacionIp.Trim();
    }
}