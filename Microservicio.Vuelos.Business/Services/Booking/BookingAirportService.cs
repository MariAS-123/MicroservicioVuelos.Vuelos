using Microservicio.Vuelos.Business.DTOs.Booking;
using Microservicio.Vuelos.Business.Exceptions;
using Microservicio.Vuelos.Business.Integrations.Interfaces;
using Microservicio.Vuelos.Business.Interfaces.Booking;

namespace Microservicio.Vuelos.Business.Services.Booking;

/// <summary>
/// Endpoint 1: GET /api/v1/booking/aeropuertos
/// Delega completamente en MS Aeropuertos via HTTP — este MS no tiene
/// la tabla de aeropuertos en su BDD.
/// </summary>
public class BookingAirportService : IBookingAirportService
{
    private readonly IAeropuertoIntegrationService _aeropuertoIntegration;

    public BookingAirportService(IAeropuertoIntegrationService aeropuertoIntegration)
    {
        _aeropuertoIntegration = aeropuertoIntegration;
    }

    public async Task<List<BookingAeropuertoResponseDto>> BuscarAeropuertosAsync(
        string? search,
        string? pais,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0 || limit > 50)
            limit = 10;

        var aeropuertos = await _aeropuertoIntegration.BuscarAeropuertosAsync(
            search, pais, limit, cancellationToken);

        return aeropuertos.Select(a => new BookingAeropuertoResponseDto
        {
            IdAeropuerto = a.IdAeropuerto,
            CodigoIata = a.CodigoIata,
            CodigoIcao = a.CodigoIcao ?? string.Empty,
            Nombre = a.Nombre,
            ZonaHoraria = a.ZonaHoraria ?? string.Empty,
            Latitud = a.Latitud ?? 0,
            Longitud = a.Longitud ?? 0,
            Estado = a.Estado ?? string.Empty,
            Ciudad = a.Ciudad is not null ? new BookingCiudadDto
            {
                IdCiudad = a.Ciudad.IdCiudad,
                Nombre = a.Ciudad.Nombre,
                ZonaHoraria = a.Ciudad.ZonaHoraria,
                Latitud = a.Ciudad.Latitud,
                Longitud = a.Ciudad.Longitud
            } : new BookingCiudadDto
            {
                IdCiudad = a.IdCiudad ?? 0,  // ← usar el integer directo
                Nombre = string.Empty,
                ZonaHoraria = string.Empty
            },
            Pais = a.Pais is not null ? new BookingPaisDto
            {
                IdPais = a.Pais.IdPais,
                CodigoIso2 = a.Pais.CodigoIso2 ?? string.Empty,
                CodigoIso3 = a.Pais.CodigoIso3 ?? string.Empty,
                Nombre = a.Pais.Nombre,
                Continente = a.Pais.Continente
            } : new BookingPaisDto
            {
                IdPais = a.IdPais ?? 0,  // ← usar el integer directo
                CodigoIso2 = string.Empty,
                CodigoIso3 = string.Empty,
                Nombre = string.Empty
            }
        }).ToList();
    }
}