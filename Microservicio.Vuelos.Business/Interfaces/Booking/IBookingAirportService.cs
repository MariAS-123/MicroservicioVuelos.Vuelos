using Microservicio.Vuelos.Business.DTOs.Booking;

namespace Microservicio.Vuelos.Business.Interfaces.Booking;

public interface IBookingAirportService
{
    /// <summary>
    /// Endpoint 1: GET /api/v1/booking/aeropuertos
    /// Lista aeropuertos activos filtrados por texto y/o país para autocompletado.
    /// </summary>
    Task<List<BookingAeropuertoResponseDto>> BuscarAeropuertosAsync(
        string? search,
        string? pais,
        int limit,
        CancellationToken cancellationToken = default);
}