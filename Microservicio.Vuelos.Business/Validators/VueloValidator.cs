using System.Text.RegularExpressions;
using Microservicio.Vuelos.Business.DTOs.Vuelo;
using Microservicio.Vuelos.Business.Exceptions;

namespace Microservicio.Vuelos.Business.Validators;

/// <summary>
/// Sin cambios respecto al monolito.
/// Valida datos de entrada antes de llegar al Service.
/// No conoce HTTP ni EF.
/// </summary>
public class VueloValidator
{
    private static readonly string[] EstadosVueloValidos =
    [
        "PROGRAMADO",
        "EN_VUELO",
        "ATERRIZADO",
        "CANCELADO",
        "DEMORADO"
    ];

    public void ValidateRequest(VueloRequestDto dto)
    {
        var errors = ValidateCommon(dto, requireNumeroVuelo: false);
        ThrowIfAny(errors, "Error de validación al crear el vuelo.");
    }

    public void ValidateUpdate(VueloUpdateRequestDto dto)
    {
        var errors = ValidateCommon(dto);
        ThrowIfAny(errors, "Error de validación al actualizar el vuelo.");
    }

    public void ValidateFilter(VueloFilterDto dto)
    {
        var errors = new List<string>();

        if (dto.IdAeropuertoOrigen.HasValue && dto.IdAeropuertoOrigen.Value <= 0)
            errors.Add("El id del aeropuerto de origen debe ser mayor que 0.");

        if (dto.IdAeropuertoDestino.HasValue && dto.IdAeropuertoDestino.Value <= 0)
            errors.Add("El id del aeropuerto de destino debe ser mayor que 0.");

        if (dto.IdAeropuertoOrigen.HasValue &&
            dto.IdAeropuertoDestino.HasValue &&
            dto.IdAeropuertoOrigen.Value == dto.IdAeropuertoDestino.Value)
            errors.Add("El aeropuerto de origen debe ser distinto al aeropuerto de destino.");

        if (!string.IsNullOrWhiteSpace(dto.NumeroVuelo))
        {
            var numeroVuelo = dto.NumeroVuelo.Trim();
            if (numeroVuelo.Length > 10)
                errors.Add("El número de vuelo no puede exceder 10 caracteres.");
            if (!Regex.IsMatch(numeroVuelo, "^[A-Za-z0-9]+$"))
                errors.Add("El número de vuelo solo puede contener letras y números.");
        }

        if (!string.IsNullOrWhiteSpace(dto.EstadoVuelo))
        {
            var estadoVuelo = dto.EstadoVuelo.Trim().ToUpperInvariant();
            if (!EstadosVueloValidos.Contains(estadoVuelo))
                errors.Add("El estado del vuelo debe ser PROGRAMADO, EN_VUELO, ATERRIZADO, CANCELADO o DEMORADO.");
        }

        if (dto.Page <= 0)
            errors.Add("La página debe ser mayor que 0.");

        if (dto.PageSize <= 0 || dto.PageSize > 200)
            errors.Add("El tamaño de página debe estar entre 1 y 200.");

        ThrowIfAny(errors, "Error de validación en el filtro de vuelos.");
    }

    public void ValidateFilterBooking(VueloFilterDto dto)
    {
        var errors = new List<string>();

        var origenDefinido = dto.IdAeropuertoOrigen.HasValue;
        var destinoDefinido = dto.IdAeropuertoDestino.HasValue;

        if (origenDefinido && dto.IdAeropuertoOrigen!.Value <= 0)
            errors.Add("El id del aeropuerto de origen debe ser mayor que 0.");

        if (destinoDefinido && dto.IdAeropuertoDestino!.Value <= 0)
            errors.Add("El id del aeropuerto de destino debe ser mayor que 0.");

        if (origenDefinido != destinoDefinido)
            errors.Add("Si envías aeropuerto de origen, también debes enviar aeropuerto de destino (y viceversa).");

        if (origenDefinido && destinoDefinido &&
            dto.IdAeropuertoOrigen!.Value == dto.IdAeropuertoDestino!.Value)
            errors.Add("El aeropuerto de origen debe ser distinto al aeropuerto de destino.");

        if (dto.Page <= 0)
            errors.Add("La página debe ser mayor que 0.");

        if (dto.PageSize <= 0 || dto.PageSize > 200)
            errors.Add("El tamaño de página debe estar entre 1 y 200.");

        ThrowIfAny(errors, "Error de validación en el filtro de vuelos.");
    }

    private static List<string> ValidateCommon(VueloRequestDto dto, bool requireNumeroVuelo = true)
    {
        var errors = new List<string>();

        if (dto.IdAeropuertoOrigen <= 0)
            errors.Add("El aeropuerto de origen es obligatorio.");

        if (dto.IdAeropuertoDestino <= 0)
            errors.Add("El aeropuerto de destino es obligatorio.");

        if (dto.IdAeropuertoOrigen > 0 &&
            dto.IdAeropuertoDestino > 0 &&
            dto.IdAeropuertoOrigen == dto.IdAeropuertoDestino)
            errors.Add("El aeropuerto de origen debe ser distinto al aeropuerto de destino.");

        if (string.IsNullOrWhiteSpace(dto.NumeroVuelo))
        {
            if (requireNumeroVuelo)
                errors.Add("El número de vuelo es obligatorio.");
        }
        else
        {
            var numeroVuelo = dto.NumeroVuelo.Trim();
            if (numeroVuelo.Length > 10)
                errors.Add("El número de vuelo no puede exceder 10 caracteres.");
            if (!Regex.IsMatch(numeroVuelo, "^[A-Za-z0-9]+$"))
                errors.Add("El número de vuelo solo puede contener letras y números.");
        }

        if (dto.FechaHoraSalida == default)
            errors.Add("La fecha y hora de salida es obligatoria.");

        if (dto.FechaHoraLlegada == default)
            errors.Add("La fecha y hora de llegada es obligatoria.");

        if (dto.FechaHoraSalida != default &&
            dto.FechaHoraLlegada != default &&
            dto.FechaHoraLlegada <= dto.FechaHoraSalida)
            errors.Add("La fecha y hora de llegada debe ser mayor que la fecha y hora de salida.");

        if (dto.DuracionMin < 0)
            errors.Add("La duración no puede ser negativa.");

        if (dto.PrecioBase < 0)
            errors.Add("El precio base no puede ser negativo.");

        if (dto.CapacidadTotal <= 0)
            errors.Add("La capacidad total debe ser mayor que 0.");

        return errors;
    }

    private static List<string> ValidateCommon(VueloUpdateRequestDto dto)
    {
        var requestEquivalent = new VueloRequestDto
        {
            IdAeropuertoOrigen = dto.IdAeropuertoOrigen,
            IdAeropuertoDestino = dto.IdAeropuertoDestino,
            NumeroVuelo = dto.NumeroVuelo,
            FechaHoraSalida = dto.FechaHoraSalida,
            FechaHoraLlegada = dto.FechaHoraLlegada,
            DuracionMin = dto.DuracionMin,
            PrecioBase = dto.PrecioBase,
            CapacidadTotal = dto.CapacidadTotal
        };
        return ValidateCommon(requestEquivalent);
    }

    private static void ThrowIfAny(List<string> errors, string message)
    {
        if (errors.Count > 0)
            throw new ValidationException(message, errors);
    }
}