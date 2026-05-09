using System.Net.Http.Json;
using Microservicio.Vuelos.Business.Integrations;
using Microservicio.Vuelos.Business.Integrations.Interfaces;

namespace Microservicio.Vuelos.Api.Integrations;

/// <summary>
/// Implementación del servicio de integración con MS Aeropuertos.
/// Vive en la capa Api — es la única capa que puede usar HttpClient y URLs.
/// Se registra en ServiceCollectionExtensions como:
///   AddScoped&lt;IAeropuertoIntegrationService, AeropuertoIntegrationService&gt;()
///
/// La URL base viene de appsettings.json:
///   "ServiciosExternos": { "AeropuertosBaseUrl": "http://localhost:5003" }
/// </summary>
public class AeropuertoIntegrationService : IAeropuertoIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AeropuertoIntegrationService> _logger;

    public AeropuertoIntegrationService(
        IHttpClientFactory httpClientFactory,
        ILogger<AeropuertoIntegrationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("aeropuertos");
        _logger = logger;
    }

    public async Task<AeropuertoIntegrationDto?> GetAeropuertoAsync(
        int idAeropuerto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/aeropuertos/{idAeropuerto}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "MS Aeropuertos retornó {StatusCode} para id_aeropuerto={IdAeropuerto}",
                    (int)response.StatusCode, idAeropuerto);
                return null;
            }

            // Deserializa solo los campos que necesita MS Vuelos
            var dto = await response.Content.ReadFromJsonAsync<AeropuertoIntegrationDto>(
                cancellationToken: cancellationToken);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al consultar MS Aeropuertos para id_aeropuerto={IdAeropuerto}",
                idAeropuerto);
            return null;
        }
    }
}