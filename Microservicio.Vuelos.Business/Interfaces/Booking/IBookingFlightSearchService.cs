using Microservicio.Vuelos.Business.DTOs.Booking;

namespace Microservicio.Vuelos.Business.Interfaces.Booking;

public interface IBookingFlightSearchService
{
    /// <summary>
    /// Endpoint 2: GET /api/v1/booking/vuelos/buscar
    /// Búsqueda principal con filtros avanzados y paginación.
    /// </summary>
    Task<BookingVueloBuscarResponseDto> BuscarVuelosAsync(
        BookingVueloBuscarQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint 3: GET /api/v1/booking/vuelos/{id_vuelo}
    /// Detalle completo con disponibilidad por clase.
    /// </summary>
    Task<BookingVueloDetalleResponseDto> ObtenerDetalleVueloAsync(
        int idVuelo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint 4: GET /api/v1/booking/vuelos/{id_vuelo}/escalas
    /// Escalas e itinerario del vuelo.
    /// </summary>
    Task<BookingEscalaResponseDto> ObtenerEscalasVueloAsync(
        int idVuelo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint 5: GET /api/v1/booking/vuelos/{id_vuelo}/asientos
    /// Mapa de asientos con filtros opcionales de clase y disponibilidad.
    /// </summary>
    Task<BookingAsientoResponseDto> ObtenerAsientosVueloAsync(
        int idVuelo,
        string? clase,
        bool? disponible,
        CancellationToken cancellationToken = default);
}