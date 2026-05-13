using Microservicio.Vuelos.Business.DTOs.Booking;

namespace Microservicio.Vuelos.Business.Interfaces.Booking;

public interface IBookingRedirectSessionService
{
    /// <summary>
    /// Endpoint 6: POST /api/v1/booking/vuelos/sesion-redirect
    /// Valida disponibilidad y genera JWT de redirección con contexto del viaje.
    /// Requiere JWT con rol INTEGRACION_BOOKING — lo valida el controller.
    /// </summary>
    Task<BookingRedirectResponseDto> GenerarSesionRedirectAsync(
        BookingRedirectRequestDto request,
        CancellationToken cancellationToken = default);
}