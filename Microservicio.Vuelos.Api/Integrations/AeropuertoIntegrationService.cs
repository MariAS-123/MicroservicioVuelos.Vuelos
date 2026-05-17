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
///
/// MÉTODOS AÑADIDOS PARA BOOKING:
///   GetAeropuertoPorIataAsync — resuelve código IATA a aeropuerto completo
///   BuscarAeropuertosAsync    — autocompletado origen/destino (endpoint 1)
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

    // ─────────────────────────────────────────────────────────────────────────
    // GET POR ID — usado por VueloService para validar origen/destino
    // ─────────────────────────────────────────────────────────────────────────

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

            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponseWrapper<AeropuertoIntegrationDto>>(
                    cancellationToken: cancellationToken);

            return wrapper?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al consultar MS Aeropuertos para id_aeropuerto={IdAeropuerto}",
                idAeropuerto);
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET POR IATA — usado por BookingFlightSearchService para resolver
    // códigos IATA (GYE, UIO) a IDs internos antes de buscar vuelos
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<AeropuertoIntegrationDto?> GetAeropuertoPorIataAsync(
    string codigoIata,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/aeropuertos?codigo_iata={Uri.EscapeDataString(codigoIata)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "MS Aeropuertos retornó {StatusCode} para codigo_iata={CodigoIata}",
                    (int)response.StatusCode, codigoIata);
                return null;
            }

            // MS Aeropuertos devuelve PagedResult con items, no lista directa
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponseWrapper<AeropuertosPagedResult>>(
                    cancellationToken: cancellationToken);

            return wrapper?.Data?.Items?.FirstOrDefault(a =>
                a.CodigoIata.Equals(codigoIata, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al consultar MS Aeropuertos para codigo_iata={CodigoIata}",
                codigoIata);
            return null;
        }
    }

    // ← Agregar esta clase privada en el mismo archivo
    private class AeropuertosPagedResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public List<AeropuertoIntegrationDto> Items { get; set; } = [];
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BUSCAR — usado por BookingAirportService para el autocompletado
    // Endpoint 1: GET /api/v1/booking/aeropuertos?search=...&pais=...&limit=...
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<List<AeropuertoIntegrationDto>> BuscarAeropuertosAsync(
        string? search,
        string? pais,
        int limit,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>();
            queryParams.Add($"page=1&page_size={limit}");

            // MS Aeropuertos usa 'nombre' y 'codigo_iata', no 'search'
            if (!string.IsNullOrWhiteSpace(search))
            {
                queryParams.Add($"nombre={Uri.EscapeDataString(search)}");
                queryParams.Add($"codigo_iata={Uri.EscapeDataString(search)}");
            }

            var url = "/api/v1/aeropuertos?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "MS Aeropuertos retornó {StatusCode} al buscar aeropuertos",
                    (int)response.StatusCode);
                return [];
            }

            // MS Aeropuertos devuelve PagedResult con items, no lista directa
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponseWrapper<AeropuertosPagedResult>>(
                    cancellationToken: cancellationToken);

            return wrapper?.Data?.Items ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al buscar aeropuertos en MS Aeropuertos (search={Search})",
                search);
            return [];
        }
    }
    // ─────────────────────────────────────────────────────────────────────────
    // HELPER PRIVADO — wrapper genérico para deserializar ApiResponse del MS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mapea el wrapper estándar { status, mensaje, data } que retorna MS Aeropuertos.
    /// Solo necesitamos el campo data — status y mensaje se ignoran aquí.
    /// </summary>
    private sealed class ApiResponseWrapper<T>
    {
        public T? Data { get; set; }
    }
}