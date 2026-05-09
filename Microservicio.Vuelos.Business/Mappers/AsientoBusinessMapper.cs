using Microservicio.Vuelos.Business.DTOs.Asiento;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Mappers;

public static class AsientoBusinessMapper
{
    public static AsientoFiltroDataModel ToFiltroDataModel(AsientoFilterDto dto)
    {
        return new AsientoFiltroDataModel
        {
            IdVuelo = dto.IdVuelo,
            Disponible = dto.Disponible,
            Clase = dto.Clase,
            NumeroAsiento = dto.NumeroAsiento,
            Posicion = dto.Posicion,
            Estado = dto.Estado,
            PageNumber = dto.Page,
            PageSize = dto.PageSize
        };
    }

    public static AsientoDataModel ToDataModel(AsientoRequestDto dto, string creadoPorUsuario)
    {
        return new AsientoDataModel
        {
            IdVuelo = dto.IdVuelo,
            NumeroAsiento = dto.NumeroAsiento,
            Clase = dto.Clase,
            Disponible = dto.Disponible,
            PrecioExtra = dto.PrecioExtra,
            Posicion = dto.Posicion,
            Estado = "ACTIVO",
            Eliminado = false,
            CreadoPorUsuario = creadoPorUsuario
        };
    }

    public static AsientoDataModel ToDataModel(int idAsiento, AsientoUpdateRequestDto dto)
    {
        return new AsientoDataModel
        {
            IdAsiento = idAsiento,
            IdVuelo = dto.IdVuelo,
            NumeroAsiento = dto.NumeroAsiento,
            Clase = dto.Clase,
            Disponible = dto.Disponible,
            PrecioExtra = dto.PrecioExtra,
            Posicion = dto.Posicion
        };
    }

    public static AsientoResponseDto ToResponseDto(AsientoDataModel model)
    {
        return new AsientoResponseDto
        {
            IdAsiento = model.IdAsiento,
            IdVuelo = model.IdVuelo,
            NumeroAsiento = model.NumeroAsiento,
            Clase = model.Clase,
            Disponible = model.Disponible,
            PrecioExtra = model.PrecioExtra,
            Posicion = model.Posicion,
            Estado = model.Estado
        };
    }

    public static List<AsientoResponseDto> ToResponseDtoList(IEnumerable<AsientoDataModel> items)
    {
        return items.Select(ToResponseDto).ToList();
    }
}   