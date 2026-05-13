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
public class BookingVuelosController : ControllerBase
{
    private readonly IBookingFlightSearchService _bookingFlightSearchService;

    public BookingVuelosController(IBookingFlightSearchService bookingFlightSearchService)
    {
        _bookingFlightSearchService = bookingFlightSearchService;
    }

    /// <summary>
    /// Endpoint 2 — Búsqueda principal de vuelos con filtros avanzados.
    /// GET /api/v1/booking/vuelos/buscar?origen=GYE&amp;destino=UIO&amp;fecha=2026-06-15
    /// </summary>
    [HttpGet("vuelos/buscar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponse<object>>> BuscarVuelos(
        [FromQuery] BookingVueloBuscarQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingFlightSearchService.BuscarVuelosAsync(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result, "Operacion exitosa"));
    }

    /// <summary>
    /// Endpoint 3 — Detalle completo de un vuelo seleccionado.
    /// GET /api/v1/booking/vuelos/{id_vuelo}
    /// </summary>
    [HttpGet("vuelos/{id_vuelo:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> GetDetalleVuelo(
        int id_vuelo,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingFlightSearchService.ObtenerDetalleVueloAsync(
            id_vuelo, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result, "Operacion exitosa"));
    }

    /// <summary>
    /// Endpoint 4 — Escalas e itinerario del vuelo.
    /// GET /api/v1/booking/vuelos/{id_vuelo}/escalas
    /// </summary>
    [HttpGet("vuelos/{id_vuelo:int}/escalas")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> GetEscalas(
        int id_vuelo,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingFlightSearchService.ObtenerEscalasVueloAsync(
            id_vuelo, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result, "Operacion exitosa"));
    }

    /// <summary>
    /// Endpoint 5 — Mapa de asientos con disponibilidad.
    /// GET /api/v1/booking/vuelos/{id_vuelo}/asientos?clase=ECONOMICA&amp;disponible=true
    /// </summary>
    [HttpGet("vuelos/{id_vuelo:int}/asientos")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> GetAsientos(
        int id_vuelo,
        [FromQuery] string? clase,
        [FromQuery] bool? disponible,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingFlightSearchService.ObtenerAsientosVueloAsync(
            id_vuelo, clase, disponible, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result, "Operacion exitosa"));
    }
}