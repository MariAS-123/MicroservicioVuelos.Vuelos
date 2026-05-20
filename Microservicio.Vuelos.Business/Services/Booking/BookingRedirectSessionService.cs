using Microservicio.Vuelos.Business.DTOs.Booking;
using Microservicio.Vuelos.Business.Exceptions;
using Microservicio.Vuelos.Business.Integrations;
using Microservicio.Vuelos.Business.Integrations.Interfaces;
using Microservicio.Vuelos.Business.Interfaces.Booking;
using Microservicio.Vuelos.DataAccess.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Microservicio.Vuelos.Business.Services.Booking;

/// <summary>
/// Endpoint 6: POST /api/v1/booking/vuelos/sesion-redirect
/// Valida disponibilidad y genera un JWT de redirección con contexto del viaje.
/// El token tiene validez de 900 segundos (15 minutos) — fijo según contrato.
/// </summary>
public class BookingRedirectSessionService : IBookingRedirectSessionService
{
    private readonly VueloQueryRepository _vueloQuery;
    private readonly IAeropuertoIntegrationService _aeropuertoIntegration;
    private readonly IConfiguration _configuration;

    public BookingRedirectSessionService(
        VueloQueryRepository vueloQuery,
        IAeropuertoIntegrationService aeropuertoIntegration,
        IConfiguration configuration)
    {
        _vueloQuery = vueloQuery;
        _aeropuertoIntegration = aeropuertoIntegration;
        _configuration = configuration;
    }

    public async Task<BookingRedirectResponseDto> GenerarSesionRedirectAsync(
        BookingRedirectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validar vuelo
        var vuelo = await _vueloQuery.ObtenerDetalleBookingAsync(
            request.IdVuelo, cancellationToken);

        if (vuelo == null)
            throw new NotFoundException($"Vuelo con id {request.IdVuelo} no encontrado.");

        if (vuelo.EstadoVuelo != "PROGRAMADO")
            throw new ValidationException(
                $"El vuelo {request.IdVuelo} no está disponible (estado: {vuelo.EstadoVuelo}).");

        // Enriquecer aeropuertos
        var tareaOrigen = _aeropuertoIntegration.GetAeropuertoAsync(vuelo.IdAeropuertoOrigen, cancellationToken);
        var tareaDestino = _aeropuertoIntegration.GetAeropuertoAsync(vuelo.IdAeropuertoDestino, cancellationToken);
        await Task.WhenAll(tareaOrigen, tareaDestino);

        var origen = tareaOrigen.Result;
        var destino = tareaDestino.Result;

        // Generar token y URL de redirección
        var expiracion = DateTime.UtcNow.AddSeconds(900);
        var redirectToken = GenerarRedirectToken(request, vuelo, expiracion);
        var baseUrl = _configuration["ServiciosExternos:AerolineaBaseUrl"]
                            ?? "https://aerolinea.com";
        var redirectUrl = $"{baseUrl}/reservar?token={redirectToken}&retorno={Uri.EscapeDataString(request.UrlRetorno)}";

        return new BookingRedirectResponseDto
        {
            Token = redirectToken,
            UrlRedirect = redirectUrl,
            Expiracion = expiracion
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPER PRIVADO — genera el JWT de redirección
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerarRedirectToken(
        BookingRedirectRequestDto request,
        VueloQueryRepository.BookingVueloDetalleCompletoDto vuelo,
        DateTime expiracion)
    {
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new BusinessException("No se encontró JwtSettings:Secret.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new("id_vuelo",    vuelo.IdVuelo.ToString()),
        new("url_retorno", request.UrlRetorno),
    };

        foreach (var idAsiento in request.IdAsientos)
            claims.Add(new Claim("id_asiento", idAsiento.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}