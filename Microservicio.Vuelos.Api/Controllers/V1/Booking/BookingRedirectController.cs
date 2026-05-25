using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicio.Vuelos.Api.Models.Common;
using Microservicio.Vuelos.Business.DTOs.Booking;
using Microservicio.Vuelos.Business.Interfaces.Booking;

namespace Microservicio.Vuelos.Api.Controllers.V1.Booking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/booking")]
[Produces("application/json")]
public class BookingRedirectController : ControllerBase
{
    private readonly IBookingRedirectSessionService _bookingRedirectSessionService;

    public BookingRedirectController(IBookingRedirectSessionService bookingRedirectSessionService)
    {
        _bookingRedirectSessionService = bookingRedirectSessionService;
    }

    /// <summary>
    /// Endpoint 6 — Genera token seguro de redirección a la aerolínea.
    /// POST /api/v1/booking/vuelos/sesion-redirect
    /// Requiere JWT con rol INTEGRACION_BOOKING.
    /// </summary>
    [HttpPost("vuelos/sesion-redirect")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponse<object>>> GenerarSesionRedirect(
        [FromBody] BookingRedirectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingRedirectSessionService.GenerarSesionRedirectAsync(
            request, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result, "Sesion generada exitosamente"));
    }
}