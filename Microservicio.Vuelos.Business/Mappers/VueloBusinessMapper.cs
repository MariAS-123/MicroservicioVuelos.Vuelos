using Microservicio.Vuelos.Business.DTOs.Vuelo;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Mappers;

/// <summary>
/// Sin cambios respecto al monolito.
/// Traduce DTOs ↔ DataModels. No conoce EF ni HTTP.
/// </summary>
public static class VueloBusinessMapper
{
    public static VueloFiltroDataModel ToFiltroDataModel(VueloFilterDto dto)
    {
        return new VueloFiltroDataModel
        {
            IdAeropuertoOrigen = dto.IdAeropuertoOrigen,
            IdAeropuertoDestino = dto.IdAeropuertoDestino,
            NumeroVuelo = dto.NumeroVuelo,
            EstadoVuelo = dto.EstadoVuelo,
            Estado = dto.Estado,
            // Busca todo el día: desde 00:00:00 hasta 23:59:59
            FechaSalidaDesde = dto.FechaSalida.HasValue
                ? dto.FechaSalida.Value.Date
                : null,
            FechaSalidaHasta = dto.FechaSalida.HasValue
                ? dto.FechaSalida.Value.Date.AddDays(1).AddSeconds(-1)
                : null,
            PageNumber = dto.Page,
            PageSize = dto.PageSize
        };
    }

    public static VueloDataModel ToDataModel(VueloRequestDto dto, string creadoPorUsuario)
    {
        return new VueloDataModel
        {
            IdAeropuertoOrigen = dto.IdAeropuertoOrigen,
            IdAeropuertoDestino = dto.IdAeropuertoDestino,
            NumeroVuelo = dto.NumeroVuelo,
            FechaHoraSalida = dto.FechaHoraSalida,
            FechaHoraLlegada = dto.FechaHoraLlegada,
            DuracionMin = dto.DuracionMin,
            PrecioBase = dto.PrecioBase,
            CapacidadTotal = dto.CapacidadTotal,
            EstadoVuelo = "PROGRAMADO",
            Estado = "ACTIVO",
            Eliminado = false,
            CreadoPorUsuario = creadoPorUsuario
        };
    }

    public static VueloDataModel ToDataModel(int idVuelo, VueloUpdateRequestDto dto)
    {
        return new VueloDataModel
        {
            IdVuelo = idVuelo,
            IdAeropuertoOrigen = dto.IdAeropuertoOrigen,
            IdAeropuertoDestino = dto.IdAeropuertoDestino,
            NumeroVuelo = dto.NumeroVuelo,
            FechaHoraSalida = dto.FechaHoraSalida,
            FechaHoraLlegada = dto.FechaHoraLlegada,
            DuracionMin = dto.DuracionMin,
            PrecioBase = dto.PrecioBase,
            CapacidadTotal = dto.CapacidadTotal,
            EstadoVuelo = dto.EstadoVuelo,
            Estado = "ACTIVO"
        };
    }

    public static VueloResponseDto ToResponseDto(VueloDataModel model)
    {
        return new VueloResponseDto
        {
            IdVuelo = model.IdVuelo,
            NumeroVuelo = model.NumeroVuelo,
            IdAeropuertoOrigen = model.IdAeropuertoOrigen,
            IdAeropuertoDestino = model.IdAeropuertoDestino,
            FechaHoraSalida = model.FechaHoraSalida,
            FechaHoraLlegada = model.FechaHoraLlegada,
            DuracionMin = model.DuracionMin,
            PrecioBase = model.PrecioBase,
            CapacidadTotal = model.CapacidadTotal,
            EstadoVuelo = model.EstadoVuelo,
            Estado = model.Estado
        };
    }

    public static List<VueloResponseDto> ToResponseDtoList(IEnumerable<VueloDataModel> items)
    {
        return items.Select(ToResponseDto).ToList();
    }
}