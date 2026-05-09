using Microservicio.Vuelos.Business.DTOs.Asiento;
using Microservicio.Vuelos.Business.Exceptions;

namespace Microservicio.Vuelos.Business.Validators;

public class AsientoValidator
{
    private static readonly string[] ClasesValidas = ["ECONOMICA", "EJECUTIVA", "PRIMERA"];
    private static readonly string[] PosicionesValidas = ["VENTANA", "PASILLO", "CENTRO"];
    private static readonly string[] EstadosValidos = ["ACTIVO", "INACTIVO"];

    public void ValidateRequest(AsientoRequestDto dto)
    {
        var errors = ValidateCommon(dto);
        ThrowIfAny(errors, "Error de validación al crear el asiento.");
    }

    public void ValidateUpdate(AsientoUpdateRequestDto dto)
    {
        var errors = ValidateCommon(dto);
        ThrowIfAny(errors, "Error de validación al actualizar el asiento.");
    }

    public void ValidateFilter(AsientoFilterDto dto)
    {
        var errors = new List<string>();

        if (dto.IdVuelo.HasValue && dto.IdVuelo.Value <= 0)
            errors.Add("El id del vuelo debe ser mayor que 0.");

        if (!string.IsNullOrWhiteSpace(dto.Clase))
        {
            var clase = dto.Clase.Trim().ToUpperInvariant();
            if (!ClasesValidas.Contains(clase))
                errors.Add("La clase debe ser ECONOMICA, EJECUTIVA o PRIMERA.");
        }

        if (!string.IsNullOrWhiteSpace(dto.NumeroAsiento) && dto.NumeroAsiento.Trim().Length > 10)
            errors.Add("El número de asiento no puede exceder 10 caracteres.");

        if (!string.IsNullOrWhiteSpace(dto.Posicion))
        {
            var posicion = dto.Posicion.Trim().ToUpperInvariant();
            if (!PosicionesValidas.Contains(posicion))
                errors.Add("La posición debe ser VENTANA, PASILLO o CENTRO.");
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

        ThrowIfAny(errors, "Error de validación en el filtro de asientos.");
    }

    private static List<string> ValidateCommon(AsientoRequestDto dto)
    {
        var errors = new List<string>();

        if (dto.IdVuelo <= 0)
            errors.Add("El vuelo es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.NumeroAsiento))
            errors.Add("El número de asiento es obligatorio.");
        else if (dto.NumeroAsiento.Trim().Length > 10)
            errors.Add("El número de asiento no puede exceder 10 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.Clase))
        {
            errors.Add("La clase es obligatoria.");
        }
        else
        {
            var clase = dto.Clase.Trim().ToUpperInvariant();
            if (!ClasesValidas.Contains(clase))
                errors.Add("La clase debe ser ECONOMICA, EJECUTIVA o PRIMERA.");
        }

        if (dto.PrecioExtra < 0)
            errors.Add("El precio extra no puede ser negativo.");

        if (!string.IsNullOrWhiteSpace(dto.Posicion))
        {
            var posicion = dto.Posicion.Trim().ToUpperInvariant();
            if (!PosicionesValidas.Contains(posicion))
                errors.Add("La posición debe ser VENTANA, PASILLO o CENTRO.");
        }

        return errors;
    }

    private static List<string> ValidateCommon(AsientoUpdateRequestDto dto)
    {
        var requestEquivalent = new AsientoRequestDto
        {
            IdVuelo = dto.IdVuelo,
            NumeroAsiento = dto.NumeroAsiento,
            Clase = dto.Clase,
            Disponible = dto.Disponible,
            PrecioExtra = dto.PrecioExtra,
            Posicion = dto.Posicion
        };
        return ValidateCommon(requestEquivalent);
    }

    private static void ThrowIfAny(List<string> errors, string message)
    {
        if (errors.Count > 0)
            throw new ValidationException(message, errors);
    }
}