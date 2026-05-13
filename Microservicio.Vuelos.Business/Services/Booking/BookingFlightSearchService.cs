using Microservicio.Vuelos.Business.DTOs.Booking;
using Microservicio.Vuelos.Business.Exceptions;
using Microservicio.Vuelos.Business.Integrations;
using Microservicio.Vuelos.Business.Integrations.Interfaces;
using Microservicio.Vuelos.Business.Interfaces.Booking;
using Microservicio.Vuelos.DataAccess.Queries;

namespace Microservicio.Vuelos.Business.Services.Booking;

/// <summary>
/// Endpoints 2, 3, 4 y 5 del contrato Booking.
/// Solo lectura — nunca escribe en BDD.
/// Enriquece los IDs de aeropuerto con datos del MS Aeropuertos via HTTP.
/// </summary>
public class BookingFlightSearchService : IBookingFlightSearchService
{
    private readonly VueloQueryRepository _vueloQuery;
    private readonly AsientoQueryRepository _asientoQuery;
    private readonly IAeropuertoIntegrationService _aeropuertoIntegration;

    public BookingFlightSearchService(
        VueloQueryRepository vueloQuery,
        AsientoQueryRepository asientoQuery,
        IAeropuertoIntegrationService aeropuertoIntegration)
    {
        _vueloQuery = vueloQuery;
        _asientoQuery = asientoQuery;
        _aeropuertoIntegration = aeropuertoIntegration;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ENDPOINT 2 — GET /api/v1/booking/vuelos/buscar
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<BookingVueloBuscarResponseDto> BuscarVuelosAsync(
        BookingVueloBuscarQueryDto query,
        CancellationToken cancellationToken = default)
    {
        // Validar params obligatorios
        if (string.IsNullOrWhiteSpace(query.Origen))
            throw new ValidationException("El parámetro origen es requerido.");

        if (string.IsNullOrWhiteSpace(query.Destino))
            throw new ValidationException("El parámetro destino es requerido.");

        if (query.Origen.Equals(query.Destino, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("El aeropuerto de origen no puede ser igual al de destino.");

        if (query.Fecha < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ValidationException("fecha no puede ser anterior a la fecha actual.");

        // Resolver códigos IATA a IDs llamando al MS Aeropuertos
        var aeropuertoOrigen = await _aeropuertoIntegration.GetAeropuertoPorIataAsync(
            query.Origen, cancellationToken);
        if (aeropuertoOrigen == null)
            throw new NotFoundException($"No se encontró el aeropuerto con código IATA '{query.Origen}'.");

        var aeropuertoDestino = await _aeropuertoIntegration.GetAeropuertoPorIataAsync(
            query.Destino, cancellationToken);
        if (aeropuertoDestino == null)
            throw new NotFoundException($"No se encontró el aeropuerto con código IATA '{query.Destino}'.");

        // Resolver aeropuerto de escala si viene en el filtro
        int? idAeropuertoEscala = null;
        if (!string.IsNullOrWhiteSpace(query.AeropuertoEscala))
        {
            var aeropuertoEscala = await _aeropuertoIntegration.GetAeropuertoPorIataAsync(
                query.AeropuertoEscala, cancellationToken);
            if (aeropuertoEscala != null)
                idAeropuertoEscala = aeropuertoEscala.IdAeropuerto;
        }

        // Construir filtros para el QueryRepository
        var filtros = new VueloQueryRepository.BookingVueloBuscarFiltrosDto
        {
            IdAeropuertoOrigen = aeropuertoOrigen.IdAeropuerto,
            IdAeropuertoDestino = aeropuertoDestino.IdAeropuerto,
            Fecha = query.Fecha.ToDateTime(TimeOnly.MinValue),
            Clase = query.Clase,
            Escalas = query.Escalas,
            HoraSalidaDesde = query.HoraSalidaDesde,
            HoraSalidaHasta = query.HoraSalidaHasta,
            HoraLlegadaDesde = query.HoraLlegadaDesde,
            HoraLlegadaHasta = query.HoraLlegadaHasta,
            DuracionMaxMin = query.DuracionMaxMin,
            EscalaDuracionMax = query.EscalaDuracionMax,
            IdAeropuertoEscala = idAeropuertoEscala,
            PrecioMin = query.PrecioMin,
            PrecioMax = query.PrecioMax,
            OrdenarPor = query.OrdenarPor,
            OrdenDir = query.OrdenDir,
            Page = query.Page <= 0 ? 1 : query.Page,
            Limit = query.Limit is <= 0 or > 100 ? 20 : query.Limit
        };

        var resultado = await _vueloQuery.BuscarDisponiblesBookingAsync(filtros, cancellationToken);

        // Mapear enriqueciendo con datos de aeropuertos
        var items = resultado.Vuelos.Select(v => new BookingVueloItemDto
        {
            IdVuelo = v.IdVuelo,
            NumeroVuelo = v.NumeroVuelo,
            FechaHoraSalida = v.FechaHoraSalida,
            FechaHoraLlegada = v.FechaHoraLlegada,
            DuracionMin = v.DuracionMin,
            NumEscalas = v.NumEscalas,
            ClaseSolicitada = query.Clase,
            PrecioBase = v.PrecioBase,
            PrecioTotal = v.PrecioBase,   // sin pasajeros en este endpoint — ver contrato
            AsientosDisponibles = v.AsientosDisponibles,
            EstadoVuelo = v.EstadoVuelo,
            Origen = new BookingAeropuertoCortoDto
            {
                IdAeropuerto = aeropuertoOrigen.IdAeropuerto,
                CodigoIata = aeropuertoOrigen.CodigoIata,
                Nombre = aeropuertoOrigen.Nombre,
                Ciudad = aeropuertoOrigen.Ciudad.Nombre,
                Pais = aeropuertoOrigen.Pais.Nombre
            },
            Destino = new BookingAeropuertoCortoDto
            {
                IdAeropuerto = aeropuertoDestino.IdAeropuerto,
                CodigoIata = aeropuertoDestino.CodigoIata,
                Nombre = aeropuertoDestino.Nombre,
                Ciudad = aeropuertoDestino.Ciudad.Nombre,
                Pais = aeropuertoDestino.Pais.Nombre
            }
        }).ToList();

        return new BookingVueloBuscarResponseDto
        {
            Meta = new BookingVueloBuscarMetaDto
            {
                Total = resultado.Total,
                Page = resultado.Page,
                Limit = resultado.Limit,
                Moneda = "USD"
            },
            Data = items
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ENDPOINT 3 — GET /api/v1/booking/vuelos/{id_vuelo}
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<BookingVueloDetalleResponseDto> ObtenerDetalleVueloAsync(
        int idVuelo,
        CancellationToken cancellationToken = default)
    {
        if (idVuelo <= 0)
            throw new ValidationException("El id del vuelo debe ser mayor que 0.");

        var vuelo = await _vueloQuery.ObtenerDetalleBookingAsync(idVuelo, cancellationToken);
        if (vuelo == null)
            throw new NotFoundException($"Vuelo con id {idVuelo} no encontrado.");

        // Enriquecer aeropuertos en paralelo
        var tareaOrigen = _aeropuertoIntegration.GetAeropuertoAsync(vuelo.IdAeropuertoOrigen, cancellationToken);
        var tareaDestino = _aeropuertoIntegration.GetAeropuertoAsync(vuelo.IdAeropuertoDestino, cancellationToken);
        await Task.WhenAll(tareaOrigen, tareaDestino);

        var origen = tareaOrigen.Result;
        var destino = tareaDestino.Result;

        if (origen == null)
            throw new NotFoundException($"Aeropuerto de origen con id {vuelo.IdAeropuertoOrigen} no encontrado.");
        if (destino == null)
            throw new NotFoundException($"Aeropuerto de destino con id {vuelo.IdAeropuertoDestino} no encontrado.");

        return new BookingVueloDetalleResponseDto
        {
            IdVuelo = vuelo.IdVuelo,
            NumeroVuelo = vuelo.NumeroVuelo,
            FechaHoraSalida = vuelo.FechaHoraSalida,
            FechaHoraLlegada = vuelo.FechaHoraLlegada,
            DuracionMin = vuelo.DuracionMin,
            PrecioBase = vuelo.PrecioBase,
            CapacidadTotal = vuelo.CapacidadTotal,
            EstadoVuelo = vuelo.EstadoVuelo,
            Origen = MapearAeropuertoDetalle(origen),
            Destino = MapearAeropuertoDetalle(destino),
            DisponibilidadPorClase = vuelo.DisponibilidadPorClase.Select(d =>
                new BookingDisponibilidadClaseDto
                {
                    Clase = d.Clase,
                    AsientosDisponibles = d.AsientosDisponibles,
                    PrecioBase = d.PrecioBase
                }).ToList()
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ENDPOINT 4 — GET /api/v1/booking/vuelos/{id_vuelo}/escalas
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<BookingEscalaResponseDto> ObtenerEscalasVueloAsync(
        int idVuelo,
        CancellationToken cancellationToken = default)
    {
        if (idVuelo <= 0)
            throw new ValidationException("El id del vuelo debe ser mayor que 0.");

        var resultado = await _vueloQuery.ObtenerEscalasBookingAsync(idVuelo, cancellationToken);
        if (resultado == null)
            throw new NotFoundException($"Vuelo con id {idVuelo} no encontrado.");

        // Enriquecer cada aeropuerto de escala
        var escalasEnriquecidas = new List<BookingEscalaItemDto>();

        foreach (var escala in resultado.Escalas)
        {
            var aeropuerto = await _aeropuertoIntegration.GetAeropuertoAsync(
                escala.IdAeropuerto, cancellationToken);

            escalasEnriquecidas.Add(new BookingEscalaItemDto
            {
                IdEscala = escala.IdEscala,
                Orden = escala.Orden,
                FechaHoraLlegada = escala.FechaHoraLlegada,
                FechaHoraSalida = escala.FechaHoraSalida,
                DuracionMin = escala.DuracionMin,
                TipoEscala = escala.TipoEscala,
                Terminal = escala.Terminal,
                Puerta = escala.Puerta,
                Observaciones = escala.Observaciones,
                Aeropuerto = aeropuerto == null ? new BookingAeropuertoEscalaDto
                {
                    IdAeropuerto = escala.IdAeropuerto,
                    CodigoIata = "???",
                    CodigoIcao = string.Empty,
                    Nombre = "Aeropuerto no disponible",
                    Ciudad = string.Empty,
                    Pais = string.Empty
                } : new BookingAeropuertoEscalaDto
                {
                    IdAeropuerto = aeropuerto.IdAeropuerto,
                    CodigoIata = aeropuerto.CodigoIata,
                    CodigoIcao = aeropuerto.CodigoIcao,
                    Nombre = aeropuerto.Nombre,
                    Ciudad = aeropuerto.Ciudad.Nombre,
                    Pais = aeropuerto.Pais.Nombre
                }
            });
        }

        return new BookingEscalaResponseDto
        {
            IdVuelo = resultado.IdVuelo,
            NumeroVuelo = resultado.NumeroVuelo,
            NumEscalas = resultado.NumEscalas,
            Escalas = escalasEnriquecidas
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ENDPOINT 5 — GET /api/v1/booking/vuelos/{id_vuelo}/asientos
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<BookingAsientoResponseDto> ObtenerAsientosVueloAsync(
        int idVuelo,
        string? clase,
        bool? disponible,
        CancellationToken cancellationToken = default)
    {
        if (idVuelo <= 0)
            throw new ValidationException("El id del vuelo debe ser mayor que 0.");

        // Obtener mapa completo y resumen en paralelo
        var tareaAsientos = _asientoQuery.ObtenerMapaPorVueloAsync(idVuelo, cancellationToken);
        var tareaResumen = _asientoQuery.ObtenerResumenOcupacionAsync(idVuelo, cancellationToken);
        await Task.WhenAll(tareaAsientos, tareaResumen);

        var asientos = tareaAsientos.Result;
        var resumen = tareaResumen.Result;

        if (resumen == null)
            throw new NotFoundException($"Vuelo con id {idVuelo} no encontrado.");

        // Aplicar filtros opcionales en memoria
        var asientosFiltrados = asientos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(clase))
            asientosFiltrados = asientosFiltrados.Where(a =>
                a.Clase.Equals(clase, StringComparison.OrdinalIgnoreCase));

        if (disponible.HasValue)
            asientosFiltrados = asientosFiltrados.Where(a => a.Disponible == disponible.Value);

        // Obtener número de vuelo
        var detalle = await _vueloQuery.ObtenerDetalleBookingAsync(idVuelo, cancellationToken);

        return new BookingAsientoResponseDto
        {
            IdVuelo = idVuelo,
            NumeroVuelo = detalle?.NumeroVuelo ?? string.Empty,
            Resumen = new BookingAsientoResumenDto
            {
                TotalAsientos = resumen.TotalAsientos,
                Disponibles = resumen.Disponibles,
                Ocupados = resumen.Ocupados
            },
            Asientos = asientosFiltrados.Select(a => new BookingAsientoItemDto
            {
                IdAsiento = a.IdAsiento,
                NumeroAsiento = a.NumeroAsiento,
                Clase = a.Clase,
                Disponible = a.Disponible,
                PrecioExtra = a.PrecioExtra,
                Posicion = a.Posicion
            }).ToList()
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────

    private static BookingAeropuertoDetalleDto MapearAeropuertoDetalle(AeropuertoIntegrationDto a) =>
        new()
        {
            IdAeropuerto = a.IdAeropuerto,
            CodigoIata = a.CodigoIata,
            CodigoIcao = a.CodigoIcao,
            Nombre = a.Nombre,
            ZonaHoraria = a.ZonaHoraria,
            Latitud = a.Latitud,
            Longitud = a.Longitud,
            Ciudad = new BookingCiudadDto
            {
                IdCiudad = a.Ciudad.IdCiudad,
                Nombre = a.Ciudad.Nombre,
                ZonaHoraria = a.Ciudad.ZonaHoraria,
                Latitud = a.Ciudad.Latitud,
                Longitud = a.Ciudad.Longitud
            },
            Pais = new BookingPaisDto
            {
                IdPais = a.Pais.IdPais,
                CodigoIso2 = a.Pais.CodigoIso2,
                CodigoIso3 = a.Pais.CodigoIso3,
                Nombre = a.Pais.Nombre,
                Continente = a.Pais.Continente
            }
        };
}