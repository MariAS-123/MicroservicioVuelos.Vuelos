using Microservicio.Vuelos.Business.DTOs.Escala;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Mappers;

public static class EscalaBusinessMapper
{
    public static EscalaFiltroDataModel ToFiltroDataModel(EscalaFilterDto dto)
    {
        return new EscalaFiltroDataModel
        {
            IdVuelo = dto.IdVuelo,
            IdAeropuerto = dto.IdAeropuerto,
            Orden = dto.Orden,
            TipoEscala = dto.TipoEscala,
            Estado = dto.Estado,
            PageNumber = dto.Page,
            PageSize = dto.PageSize
        };
    }

    public static EscalaDataModel ToDataModel(EscalaRequestDto dto, string creadoPorUsuario)
    {
        return new EscalaDataModel
        {
            IdVuelo = dto.IdVuelo,
            IdAeropuerto = dto.IdAeropuerto,
            Orden = dto.Orden,
            FechaHoraLlegada = dto.FechaHoraLlegada,
            FechaHoraSalida = dto.FechaHoraSalida,
            DuracionMin = dto.DuracionMin,
            TipoEscala = dto.TipoEscala,
            Terminal = dto.Terminal,
            Puerta = dto.Puerta,
            Observaciones = dto.Observaciones,
            Estado = "ACTIVO",
            Eliminado = false,
            CreadoPorUsuario = creadoPorUsuario
        };
    }

    public static EscalaDataModel ToDataModel(int idEscala, EscalaUpdateRequestDto dto)
    {
        return new EscalaDataModel
        {
            IdEscala = idEscala,
            IdVuelo = dto.IdVuelo,
            IdAeropuerto = dto.IdAeropuerto,
            Orden = dto.Orden,
            FechaHoraLlegada = dto.FechaHoraLlegada,
            FechaHoraSalida = dto.FechaHoraSalida,
            DuracionMin = dto.DuracionMin,
            TipoEscala = dto.TipoEscala,
            Terminal = dto.Terminal,
            Puerta = dto.Puerta,
            Observaciones = dto.Observaciones
        };
    }

    public static EscalaResponseDto ToResponseDto(EscalaDataModel model)
    {
        return new EscalaResponseDto
        {
            IdEscala = model.IdEscala,
            IdVuelo = model.IdVuelo,
            IdAeropuerto = model.IdAeropuerto,
            Orden = model.Orden,
            FechaHoraLlegada = model.FechaHoraLlegada,
            FechaHoraSalida = model.FechaHoraSalida,
            DuracionMin = model.DuracionMin,
            TipoEscala = model.TipoEscala,
            Terminal = model.Terminal,
            Puerta = model.Puerta,
            Observaciones = model.Observaciones,
            Estado = model.Estado
        };
    }

    public static List<EscalaResponseDto> ToResponseDtoList(IEnumerable<EscalaDataModel> items)
    {
        return items.Select(ToResponseDto).ToList();
    }
}