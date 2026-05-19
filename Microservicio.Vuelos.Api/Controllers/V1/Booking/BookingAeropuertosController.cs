using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicio.Vuelos.Api.Models.Common;
using Microservicio.Vuelos.Business.Interfaces.Booking;

namespace Microservicio.Vuelos.Api.Controllers.V1.Booking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/booking")]
[Produces("application/json")]
public class BookingAeropuertosController : ControllerBase
{
    private readonly IBookingAirportService _bookingAirportService;

    public BookingAeropuertosController(IBookingAirportService bookingAirportService)
    {
        _bookingAirportService = bookingAirportService;
    }

    /// <summary>
    /// Endpoint 1 — Lista aeropuertos activos para autocompletado origen/destino.
    /// GET /api/v1/booking/aeropuertos?search=Guaya&amp;pais=EC&amp;limit=5
    /// </summary>
    [HttpGet("aeropuertos")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> BuscarAeropuertos(
        [FromQuery] string? nombre,
        [FromQuery] int? idPais,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingAirportService.BuscarAeropuertosAsync(
            nombre, idPais?.ToString(), limit, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result, "Operacion exitosa"));
    }
}