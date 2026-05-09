using Microservicio.Vuelos.Business.DTOs.Escala;
using Microservicio.Vuelos.Business.Exceptions;

namespace Microservicio.Vuelos.Business.Validators;

public class EscalaValidator
{
    private static readonly string[] TiposEscalaValidos = ["TECNICA", "COMERCIAL"];
    private static readonly string[] EstadosValidos = ["ACTIVO", "INACTIVO"];

    public void ValidateRequest(EscalaRequestDto dto)
    {
        var errors = ValidateCommon(dto);
        ThrowIfAny(errors, "Error de validación al crear la escala.");
    }

    public void ValidateUpdate(EscalaUpdateRequestDto dto)
    {
        var errors = ValidateCommon(dto);
        ThrowIfAny(errors, "Error de validación al actualizar la escala.");
    }

    public void ValidateFilter(EscalaFilterDto dto)
    {
        var errors = new List<string>();

        if (dto.IdVuelo.HasValue && dto.IdVuelo.Value <= 0)
            errors.Add("El id del vuelo debe ser mayor que 0.");

        if (dto.IdAeropuerto.HasValue && dto.IdAeropuerto.Value <= 0)
            errors.Add("El id del aeropuerto debe ser mayor que 0.");

        if (dto.Orden.HasValue && dto.Orden.Value <= 0)
            errors.Add("El orden debe ser mayor que 0.");

        if (!string.IsNullOrWhiteSpace(dto.TipoEscala))
        {
            var tipoEscala = dto.TipoEscala.Trim().ToUpperInvariant();
            if (!TiposEscalaValidos.Contains(tipoEscala))
                errors.Add("El tipo de escala debe ser TECNICA o COMERCIAL.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Estado))
        {
            var estado = dto.Estado.Trim().ToUpperInvariant();
            if (!EstadosValidos.Contains(estado))
                errors.Add("El estado debe ser ACTIVO o INACTIVO.");
        }

        if (dto.Page <= 0)
            errors.Add("La página debe ser mayor que 0.");

        if (dto.PageSize <= 0 || dto.PageSize > 200)
            errors.Add("El tamaño de página debe estar entre 1 y 200.");

        ThrowIfAny(errors, "Error de validación en el filtro de escalas.");
    }

    private static List<string> ValidateCommon(EscalaRequestDto dto)
    {
        var errors = new List<string>();

        if (dto.IdVuelo <= 0)
            errors.Add("El vuelo es obligatorio.");

        if (dto.IdAeropuerto <= 0)
            errors.Add("El aeropuerto es obligatorio.");

        if (dto.Orden <= 0)
            errors.Add("El orden debe ser mayor que 0.");

        if (dto.FechaHoraLlegada == default)
            errors.Add("La fecha y hora de llegada es obligatoria.");

        if (dto.FechaHoraSalida == default)
            errors.Add("La fecha y hora de salida es obligatoria.");

        if (dto.FechaHoraLlegada != default &&
            dto.FechaHoraSalida != default &&
            dto.FechaHoraSalida <= dto.FechaHoraLlegada)
            errors.Add("La fecha y hora de salida debe ser mayor que la fecha y hora de llegada.");

        if (dto.DuracionMin < 0)
            errors.Add("La duración no puede ser negativa.");

        if (string.IsNullOrWhiteSpace(dto.TipoEscala))
        {
            errors.Add("El tipo de escala es obligatorio.");
        }
        else
        {
            var tipoEscala = dto.TipoEscala.Trim().ToUpperInvariant();
            if (!TiposEscalaValidos.Contains(tipoEscala))
                errors.Add("El tipo de escala debe ser TECNICA o COMERCIAL.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Terminal) && dto.Terminal.Trim().Length > 20)
            errors.Add("La terminal no puede exceder 20 caracteres.");

        if (!string.IsNullOrWhiteSpace(dto.Puerta) && dto.Puerta.Trim().Length > 20)
            errors.Add("La puerta no puede exceder 20 caracteres.");

        if (!string.IsNullOrWhiteSpace(dto.Observaciones) && dto.Observaciones.Trim().Length > 500)
            errors.Add("Las observaciones no pueden exceder 500 caracteres.");

        return errors;
    }

    private static List<string> ValidateCommon(EscalaUpdateRequestDto dto)
    {
        var requestEquivalent = new EscalaRequestDto
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
            Observaciones = dto.Observaciones
        };
        return ValidateCommon(requestEquivalent);
    }

    private static void ThrowIfAny(List<string> errors, string message)
    {
        if (errors.Count > 0)
            throw new ValidationException(message, errors);
    }
}