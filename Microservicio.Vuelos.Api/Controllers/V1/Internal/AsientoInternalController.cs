// Controllers/V1/Internal/AsientoInternalController.cs
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicio.Vuelos.Business.DTOs.Asiento;
using Microservicio.Vuelos.Business.Interfaces;

namespace Microservicio.Vuelos.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/vuelos/{id_vuelo:int}/asientos")]
[AllowAnonymous]
public class AsientoInternalController : ControllerBase
{
    private readonly IAsientoService _asientoService;

    public AsientoInternalController(IAsientoService asientoService)
    {
        _asientoService = asientoService;
    }

    /// <summary>
    /// Bloquea un asiento — llamado internamente por MS ReservasF al pagar.
    /// PATCH /api/v1/internal/vuelos/{id_vuelo}/asientos/{id_asiento}/bloquear
    /// </summary>
    [HttpPatch("{id_asiento:int}/bloquear")]
    public async Task<IActionResult> Bloquear(
        [FromRoute] int id_vuelo,
        [FromRoute] int id_asiento)
    {
        // Obtener el asiento actual para no perder sus datos
        var actual = await _asientoService.GetByIdAsync(id_asiento);
        if (actual is null)
            return NotFound(new { success = false, message = "Asiento no encontrado." });

        var result = await _asientoService.UpdateAsync(
            id_asiento,
            new AsientoUpdateRequestDto
            {
                IdVuelo = id_vuelo,
                NumeroAsiento = actual.NumeroAsiento,
                Clase = actual.Clase,
                Disponible = false,
                PrecioExtra = actual.PrecioExtra,
                Posicion = actual.Posicion
            },
            "SYSTEM_RESERVAS");

        return result is null
            ? NotFound(new { success = false, message = "No se pudo bloquear el asiento." })
            : Ok(new { success = true });
    }
}