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
        // Validar vuelo de ida
        var vueloIda = await _vueloQuery.ObtenerDetalleBookingAsync(
            request.IdVueloIda, cancellationToken);

        if (vueloIda == null)
            throw new NotFoundException($"Vuelo de ida con id {request.IdVueloIda} no encontrado.");

        if (vueloIda.EstadoVuelo != "PROGRAMADO")
            throw new ValidationException(
                $"El vuelo de ida {request.IdVueloIda} no esta disponible para reserva (estado: {vueloIda.EstadoVuelo}).");

        // Validar disponibilidad de asientos en vuelo de ida
        var disponiblesIda = vueloIda.DisponibilidadPorClase
            .FirstOrDefault(d => d.Clase.Equals(request.Clase, StringComparison.OrdinalIgnoreCase));

        if (disponiblesIda == null || disponiblesIda.AsientosDisponibles < request.Pasajeros)
            throw new ValidationException(
                $"No hay suficientes asientos {request.Clase} en el vuelo de ida {request.IdVueloIda} " +
                $"(solicitados: {request.Pasajeros}, disponibles: {disponiblesIda?.AsientosDisponibles ?? 0}).");

        // Enriquecer aeropuertos del vuelo de ida en paralelo
        var tareaOrigenIda = _aeropuertoIntegration.GetAeropuertoAsync(vueloIda.IdAeropuertoOrigen, cancellationToken);
        var tareaDestinoIda = _aeropuertoIntegration.GetAeropuertoAsync(vueloIda.IdAeropuertoDestino, cancellationToken);
        await Task.WhenAll(tareaOrigenIda, tareaDestinoIda);

        var origenIda = tareaOrigenIda.Result;
        var destinoIda = tareaDestinoIda.Result;

        // Vuelo de retorno — opcional
        VueloQueryRepository.BookingVueloDetalleCompletoDto? vueloRetorno = null;
        AeropuertoIntegrationDto? origenRetorno = null;
        AeropuertoIntegrationDto? destinoRetorno = null;

        if (request.IdVueloRetorno.HasValue)
        {
            vueloRetorno = await _vueloQuery.ObtenerDetalleBookingAsync(
                request.IdVueloRetorno.Value, cancellationToken);

            if (vueloRetorno == null)
                throw new NotFoundException($"Vuelo de retorno con id {request.IdVueloRetorno} no encontrado.");

            if (vueloRetorno.EstadoVuelo != "PROGRAMADO")
                throw new ValidationException(
                    $"El vuelo de retorno {request.IdVueloRetorno} no esta disponible para reserva (estado: {vueloRetorno.EstadoVuelo}).");

            // El origen del retorno debe ser el destino de la ida
            if (vueloRetorno.IdAeropuertoOrigen != vueloIda.IdAeropuertoDestino)
                throw new ValidationException(
                    "El origen del vuelo de retorno debe ser el destino del vuelo de ida.");

            // Validar disponibilidad en retorno
            var disponiblesRetorno = vueloRetorno.DisponibilidadPorClase
                .FirstOrDefault(d => d.Clase.Equals(request.Clase, StringComparison.OrdinalIgnoreCase));

            if (disponiblesRetorno == null || disponiblesRetorno.AsientosDisponibles < request.Pasajeros)
                throw new ValidationException(
                    $"No hay suficientes asientos {request.Clase} en el vuelo de retorno {request.IdVueloRetorno} " +
                    $"(solicitados: {request.Pasajeros}, disponibles: {disponiblesRetorno?.AsientosDisponibles ?? 0}).");

            var tareaOrigenRetorno = _aeropuertoIntegration.GetAeropuertoAsync(vueloRetorno.IdAeropuertoOrigen, cancellationToken);
            var tareaDestinoRetorno = _aeropuertoIntegration.GetAeropuertoAsync(vueloRetorno.IdAeropuertoDestino, cancellationToken);
            await Task.WhenAll(tareaOrigenRetorno, tareaDestinoRetorno);

            origenRetorno = tareaOrigenRetorno.Result;
            destinoRetorno = tareaDestinoRetorno.Result;
        }

        // Generar JWT de redirección
        var tipoViaje = request.IdVueloRetorno.HasValue ? "IDA_VUELTA" : "IDA";
        var expiracion = DateTime.UtcNow.AddSeconds(900);
        var redirectToken = GenerarRedirectToken(request, vueloIda, vueloRetorno, tipoViaje, expiracion);
        var baseUrl = _configuration["ServiciosExternos:AerolineaBaseUrl"]
                          ?? "https://aerolinea.com";
        var redirectUrl = $"{baseUrl}/reservar?token={redirectToken}";

        return new BookingRedirectResponseDto
        {
            RedirectToken = redirectToken,
            RedirectUrl = redirectUrl,
            ExpiresIn = 900,
            TipoViaje = tipoViaje,
            Contexto = new BookingRedirectContextoDto
            {
                Ida = new BookingRedirectVueloDto
                {
                    IdVuelo = vueloIda.IdVuelo,
                    NumeroVuelo = vueloIda.NumeroVuelo,
                    Origen = origenIda?.CodigoIata ?? vueloIda.IdAeropuertoOrigen.ToString(),
                    Destino = destinoIda?.CodigoIata ?? vueloIda.IdAeropuertoDestino.ToString(),
                    FechaHoraSalida = vueloIda.FechaHoraSalida,
                    FechaHoraLlegada = vueloIda.FechaHoraLlegada,
                    DuracionMin = vueloIda.DuracionMin,
                    PrecioBaseReferencial = vueloIda.PrecioBase,
                    PrecioTotalReferencial = vueloIda.PrecioBase * request.Pasajeros
                },
                Retorno = vueloRetorno == null ? null : new BookingRedirectVueloDto
                {
                    IdVuelo = vueloRetorno.IdVuelo,
                    NumeroVuelo = vueloRetorno.NumeroVuelo,
                    Origen = origenRetorno?.CodigoIata ?? vueloRetorno.IdAeropuertoOrigen.ToString(),
                    Destino = destinoRetorno?.CodigoIata ?? vueloRetorno.IdAeropuertoDestino.ToString(),
                    FechaHoraSalida = vueloRetorno.FechaHoraSalida,
                    FechaHoraLlegada = vueloRetorno.FechaHoraLlegada,
                    DuracionMin = vueloRetorno.DuracionMin,
                    PrecioBaseReferencial = vueloRetorno.PrecioBase,
                    PrecioTotalReferencial = vueloRetorno.PrecioBase * request.Pasajeros
                }
            }
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPER PRIVADO — genera el JWT de redirección
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerarRedirectToken(
        BookingRedirectRequestDto request,
        VueloQueryRepository.BookingVueloDetalleCompletoDto vueloIda,
        VueloQueryRepository.BookingVueloDetalleCompletoDto? vueloRetorno,
        string tipoViaje,
        DateTime expiracion)
    {
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new BusinessException("No se encontró JwtSettings:Secret en la configuración.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("tipo_viaje",          tipoViaje),
            new("id_vuelo_ida",        vueloIda.IdVuelo.ToString()),
            new("clase",               request.Clase),
            new("pasajeros",           request.Pasajeros.ToString()),
        };

        if (!string.IsNullOrWhiteSpace(request.ReferenciaBooking))
            claims.Add(new Claim("referencia_booking", request.ReferenciaBooking));

        if (vueloRetorno != null)
            claims.Add(new Claim("id_vuelo_retorno", vueloRetorno.IdVuelo.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}