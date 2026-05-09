using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicio.Vuelos.Api.Models.Common;
using Microservicio.Vuelos.Business.DTOs.Escala;
using Microservicio.Vuelos.Business.Interfaces;

namespace Microservicio.Vuelos.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/vuelos/{id_vuelo:int}/escalas")]
[Produces("application/json")]
[Authorize]
public class EscalaAdminController : ControllerBase
{
    private readonly IEscalaService _escalaService;

    public EscalaAdminController(IEscalaService escalaService)
    {
        _escalaService = escalaService;
    }

    // GET /vuelos/{id_vuelo}/escalas
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetPaged(
        int id_vuelo,
        [FromQuery] int? id_aeropuerto,
        [FromQuery] int? orden,
        [FromQuery] string? tipo_escala,
        [FromQuery] string? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new EscalaFilterDto
        {
            IdVuelo = id_vuelo,
            IdAeropuerto = id_aeropuerto,
            Orden = orden,
            TipoEscala = tipo_escala,
            Estado = estado,
            Page = page,
            PageSize = pageSize
        };

        var result = await _escalaService.GetPagedAsync(filter);
        return Ok(ApiResponse<object>.Ok(result, "Consulta de escalas realizada correctamente."));
    }

    // GET /vuelos/{id_vuelo}/escalas/{id_escala}
    [HttpGet("{id_escala:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<EscalaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EscalaResponseDto>>> GetById(int id_vuelo, int id_escala)
    {
        var result = await _escalaService.GetByIdAsync(id_escala);
        if (result is null || result.IdVuelo != id_vuelo)
            return NotFound(ApiResponse<EscalaResponseDto>.Fail("Escala no encontrada."));

        return Ok(ApiResponse<EscalaResponseDto>.Ok(result));
    }

    // POST /vuelos/{id_vuelo}/escalas — ADMINISTRADOR y AEROLINEA
    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR,AEROLINEA")]
    [ProducesResponseType(typeof(ApiResponse<EscalaResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<EscalaResponseDto>>> Create(
        int id_vuelo,
        [FromBody] EscalaRequestDto request)
    {
        request.IdVuelo = id_vuelo;
        var usuario = GetUsuario();
        var result = await _escalaService.CreateAsync(request, usuario);

        return CreatedAtAction(
            nameof(GetById),
            new { id_vuelo = result.IdVuelo, id_escala = result.IdEscala, version = "1" },
            ApiResponse<EscalaResponseDto>.Ok(result, "Escala creada correctamente."));
    }

    // DELETE /vuelos/{id_vuelo}/escalas/{id_escala} — ADMINISTRADOR y AEROLINEA
    [HttpDelete("{id_escala:int}")]
    [Authorize(Roles = "ADMINISTRADOR,AEROLINEA")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id_vuelo, int id_escala)
    {
        var escala = await _escalaService.GetByIdAsync(id_escala);
        if (escala is null || escala.IdVuelo != id_vuelo)
            return NotFound(ApiResponse<bool>.Fail("Escala no encontrada."));

        var usuario = GetUsuario();
        var result = await _escalaService.DeleteAsync(id_escala, usuario);
        return Ok(ApiResponse<bool>.Ok(result, "Escala eliminada correctamente."));
    }

    private string GetUsuario() =>
        User?.Identity?.Name ?? "SYSTEM";
}