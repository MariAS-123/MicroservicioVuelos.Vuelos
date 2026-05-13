namespace Microservicio.Vuelos.Business.Integrations.Interfaces;

/// <summary>
/// Contrato para consultar datos del MS Aeropuertos.
/// La capa Business SOLO conoce esta interfaz — nunca HttpClient ni URLs.
/// La implementación real con HttpClient vive en la capa Api.
/// </summary>
public interface IAeropuertoIntegrationService
{
    /// <summary>
    /// Obtiene un aeropuerto por su ID.
    /// Usado por VueloService para validar origen/destino al crear/actualizar vuelos.
    /// </summary>
    Task<AeropuertoIntegrationDto?> GetAeropuertoAsync(
        int idAeropuerto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un aeropuerto por su código IATA.
    /// Usado por el Booking para resolver origen/destino desde los query params.
    /// </summary>
    Task<AeropuertoIntegrationDto?> GetAeropuertoPorIataAsync(
        string codigoIata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca aeropuertos activos con filtro de texto y/o país.
    /// Usado por el Booking en el endpoint 1 — autocompletado origen/destino.
    /// </summary>
    Task<List<AeropuertoIntegrationDto>> BuscarAeropuertosAsync(
        string? search,
        string? pais,
        int limit,
        CancellationToken cancellationToken = default);
}