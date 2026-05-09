namespace Microservicio.Vuelos.Business.Integrations.Interfaces;

/// <summary>
/// Contrato para consultar datos del MS Aeropuertos.
/// La capa Business SOLO conoce esta interfaz — nunca HttpClient ni URLs.
/// La implementación real con HttpClient vive en la capa Api.
/// </summary>
public interface IAeropuertoIntegrationService
{
    /// <summary>
    /// Obtiene un aeropuerto por su ID consultando al MS Aeropuertos.
    /// Retorna null si el aeropuerto no existe o el MS Aeropuertos no responde.
    /// </summary>
    Task<AeropuertoIntegrationDto?> GetAeropuertoAsync(int idAeropuerto, CancellationToken cancellationToken = default);
}