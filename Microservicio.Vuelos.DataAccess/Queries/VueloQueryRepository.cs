using Microsoft.EntityFrameworkCore;
using Microservicio.Vuelos.DataAccess.Context;

namespace Microservicio.Vuelos.DataAccess.Queries
{
    /// <summary>
    /// Consultas especializadas de vuelos (búsquedas, filtros, reportes).
    /// CAMBIOS MICROSERVICIO:
    ///
    ///   BuscarDisponiblesAsync:
    ///     - Se eliminó AeropuertoOrigen = v.AeropuertoOrigen.Nombre y
    ///       AeropuertoDestino = v.AeropuertoDestino.Nombre porque
    ///       AeropuertoEntity no existe en esta BDD.
    ///     - Ahora el DTO retorna IdAeropuertoOrigen e IdAeropuertoDestino (INT).
    ///       La capa Business enriquece esos IDs llamando al MS Aeropuertos vía HTTP
    ///       si necesita mostrar el nombre.
    ///
    ///   ObtenerDetalleCompletoAsync:
    ///     - Igual: AeropuertoOrigen/Destino.Nombre → ahora son IDs.
    ///     - BoletosEmitidos = v.Boletos.Count(...) eliminado — BoletoEntity
    ///       pertenece a MS Facturación, no existe en esta BDD.
    ///     - En EscalaDto: e.Aeropuerto.Nombre → ahora es IdAeropuerto (INT).
    ///
    ///   ObtenerOcupacionPorRangoAsync: sin cambios — solo usa vuelos.vuelo y asientos.
    ///
    /// BOOKING — métodos añadidos:
    ///   BuscarDisponiblesBookingAsync: búsqueda con los 12 filtros avanzados del contrato
    ///   ObtenerDetalleBookingAsync:    detalle con disponibilidad agrupada por clase
    ///   ObtenerEscalasBookingAsync:    escalas completas con terminal, puerta y observaciones
    /// </summary>
    public class VueloQueryRepository
    {
        private readonly VuelosDbContext _context;

        public VueloQueryRepository(VuelosDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DTOs internos del QueryRepository — uso normal
        // ─────────────────────────────────────────────────────────────────────

        public class VueloDisponibleDto
        {
            public int IdVuelo { get; set; }
            public string NumeroVuelo { get; set; } = string.Empty;
            public DateTime FechaHoraSalida { get; set; }
            public DateTime FechaHoraLlegada { get; set; }
            public decimal PrecioBase { get; set; }
            public int IdAeropuertoOrigen { get; set; }
            public int IdAeropuertoDestino { get; set; }
            public int AsientosDisponibles { get; set; }
        }

        public class EscalaDto
        {
            public int Orden { get; set; }
            public int IdAeropuerto { get; set; }
            public DateTime FechaHoraLlegada { get; set; }
            public DateTime FechaHoraSalida { get; set; }
            public string TipoEscala { get; set; } = string.Empty;
        }

        public class VueloDetalleDto
        {
            public int IdVuelo { get; set; }
            public string NumeroVuelo { get; set; } = string.Empty;
            public DateTime FechaHoraSalida { get; set; }
            public DateTime FechaHoraLlegada { get; set; }
            public int DuracionMin { get; set; }
            public decimal PrecioBase { get; set; }
            public int CapacidadTotal { get; set; }
            public string EstadoVuelo { get; set; } = string.Empty;
            public int IdAeropuertoOrigen { get; set; }
            public int IdAeropuertoDestino { get; set; }
            public int AsientosDisponibles { get; set; }
            public List<EscalaDto> Escalas { get; set; } = new();
        }

        public class VueloOcupacionDto
        {
            public int IdVuelo { get; set; }
            public string NumeroVuelo { get; set; } = string.Empty;
            public int CapacidadTotal { get; set; }
            public int AsientosDisponibles { get; set; }
            public int AsientosOcupados { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DTOs internos del QueryRepository — Booking
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resultado de un vuelo en la lista de búsqueda del Booking (endpoint 2).
        /// Contiene IDs de aeropuerto — Business los enriquece con nombres via HTTP.
        /// </summary>
        public class BookingVueloBuscarDto
        {
            public int IdVuelo { get; set; }
            public string NumeroVuelo { get; set; } = string.Empty;
            public int IdAeropuertoOrigen { get; set; }
            public int IdAeropuertoDestino { get; set; }
            public DateTime FechaHoraSalida { get; set; }
            public DateTime FechaHoraLlegada { get; set; }
            public int DuracionMin { get; set; }
            public int NumEscalas { get; set; }
            public decimal PrecioBase { get; set; }
            public int AsientosDisponibles { get; set; }
            public string EstadoVuelo { get; set; } = string.Empty;
        }

        /// <summary>
        /// Disponibilidad de asientos agrupada por clase — parte del detalle (endpoint 3).
        /// </summary>
        public class BookingDisponibilidadClaseDto
        {
            public string Clase { get; set; } = string.Empty;
            public int AsientosDisponibles { get; set; }
            public decimal PrecioBase { get; set; }
        }

        /// <summary>
        /// Detalle completo de un vuelo con disponibilidad por clase (endpoint 3).
        /// </summary>
        public class BookingVueloDetalleCompletoDto
        {
            public int IdVuelo { get; set; }
            public string NumeroVuelo { get; set; } = string.Empty;
            public int IdAeropuertoOrigen { get; set; }
            public int IdAeropuertoDestino { get; set; }
            public DateTime FechaHoraSalida { get; set; }
            public DateTime FechaHoraLlegada { get; set; }
            public int DuracionMin { get; set; }
            public decimal PrecioBase { get; set; }
            public int CapacidadTotal { get; set; }
            public string EstadoVuelo { get; set; } = string.Empty;
            public List<BookingDisponibilidadClaseDto> DisponibilidadPorClase { get; set; } = new();
        }

        /// <summary>
        /// Una escala con todos los campos del contrato (endpoint 4).
        /// </summary>
        public class BookingEscalaDetalleDto
        {
            public int IdEscala { get; set; }
            public int Orden { get; set; }
            public int IdAeropuerto { get; set; }
            public DateTime FechaHoraLlegada { get; set; }
            public DateTime FechaHoraSalida { get; set; }
            public int DuracionMin { get; set; }
            public string TipoEscala { get; set; } = string.Empty;
            public string? Terminal { get; set; }
            public string? Puerta { get; set; }
            public string? Observaciones { get; set; }
        }

        /// <summary>
        /// Escalas completas de un vuelo (endpoint 4).
        /// </summary>
        public class BookingVueloEscalasDto
        {
            public int IdVuelo { get; set; }
            public string NumeroVuelo { get; set; } = string.Empty;
            public int NumEscalas { get; set; }
            public List<BookingEscalaDetalleDto> Escalas { get; set; } = new();
        }

        /// <summary>
        /// Objeto con todos los filtros posibles del endpoint 2.
        /// Business construye este objeto desde los query params del controller
        /// y lo pasa al QueryRepository — así el método queda limpio.
        /// </summary>
        public class BookingVueloBuscarFiltrosDto
        {
            // Obligatorios
            public int IdAeropuertoOrigen { get; set; }
            public int IdAeropuertoDestino { get; set; }
            public DateTime Fecha { get; set; }
            // Opcionales
            public string? Clase { get; set; }
            public int? Escalas { get; set; }
            public TimeOnly? HoraSalidaDesde { get; set; }
            public TimeOnly? HoraSalidaHasta { get; set; }
            public TimeOnly? HoraLlegadaDesde { get; set; }
            public TimeOnly? HoraLlegadaHasta { get; set; }
            public int? DuracionMaxMin { get; set; }
            public int? EscalaDuracionMax { get; set; }
            public int? IdAeropuertoEscala { get; set; }
            public decimal? PrecioMin { get; set; }
            public decimal? PrecioMax { get; set; }
            // Ordenamiento y paginación
            public string? OrdenarPor { get; set; }  // PRECIO · DURACION · SALIDA · LLEGADA
            public string OrdenDir { get; set; } = "ASC";
            public int Page { get; set; } = 1;
            public int Limit { get; set; } = 20;
        }

        /// <summary>
        /// Resultado paginado de la búsqueda del Booking.
        /// </summary>
        public class BookingVueloBuscarResultadoDto
        {
            public int Total { get; set; }
            public int Page { get; set; }
            public int Limit { get; set; }
            public List<BookingVueloBuscarDto> Vuelos { get; set; } = new();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Consultas — uso normal
        // ─────────────────────────────────────────────────────────────────────

        public async Task<List<VueloDisponibleDto>> BuscarDisponiblesAsync(
            int idAeropuertoOrigen,
            int idAeropuertoDestino,
            DateTime fecha,
            CancellationToken cancellationToken = default)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Vuelos
                .AsNoTracking()
                .Where(v =>
                    v.IdAeropuertoOrigen == idAeropuertoOrigen &&
                    v.IdAeropuertoDestino == idAeropuertoDestino &&
                    v.FechaHoraSalida >= fechaInicio &&
                    v.FechaHoraSalida < fechaFin &&
                    v.EstadoVuelo == "PROGRAMADO" &&
                    !v.Eliminado)
                .Select(v => new VueloDisponibleDto
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    FechaHoraSalida = v.FechaHoraSalida,
                    FechaHoraLlegada = v.FechaHoraLlegada,
                    PrecioBase = v.PrecioBase,
                    IdAeropuertoOrigen = v.IdAeropuertoOrigen,
                    IdAeropuertoDestino = v.IdAeropuertoDestino,
                    AsientosDisponibles = v.Asientos.Count(a => a.Disponible && !a.Eliminado)
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<VueloDetalleDto?> ObtenerDetalleCompletoAsync(
            int idVuelo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Vuelos
                .AsNoTracking()
                .Where(v => v.IdVuelo == idVuelo && !v.Eliminado)
                .Select(v => new VueloDetalleDto
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    FechaHoraSalida = v.FechaHoraSalida,
                    FechaHoraLlegada = v.FechaHoraLlegada,
                    DuracionMin = v.DuracionMin,
                    PrecioBase = v.PrecioBase,
                    CapacidadTotal = v.CapacidadTotal,
                    EstadoVuelo = v.EstadoVuelo,
                    IdAeropuertoOrigen = v.IdAeropuertoOrigen,
                    IdAeropuertoDestino = v.IdAeropuertoDestino,
                    AsientosDisponibles = v.Asientos.Count(a => a.Disponible && !a.Eliminado),
                    Escalas = v.Escalas
                        .Where(e => !e.Eliminado)
                        .OrderBy(e => e.Orden)
                        .Select(e => new EscalaDto
                        {
                            Orden = e.Orden,
                            IdAeropuerto = e.IdAeropuerto,
                            FechaHoraLlegada = e.FechaHoraLlegada,
                            FechaHoraSalida = e.FechaHoraSalida,
                            TipoEscala = e.TipoEscala
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<VueloOcupacionDto>> ObtenerOcupacionPorRangoAsync(
            DateTime desde,
            DateTime hasta,
            CancellationToken cancellationToken = default)
        {
            return await _context.Vuelos
                .AsNoTracking()
                .Where(v => v.FechaHoraSalida >= desde && v.FechaHoraSalida <= hasta && !v.Eliminado)
                .Select(v => new VueloOcupacionDto
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    CapacidadTotal = v.CapacidadTotal,
                    AsientosDisponibles = v.Asientos.Count(a => a.Disponible && !a.Eliminado),
                    AsientosOcupados = v.Asientos.Count(a => !a.Disponible && !a.Eliminado)
                })
                .ToListAsync(cancellationToken);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Consultas — Booking
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Búsqueda principal del Booking con todos los filtros avanzados del contrato.
        /// Endpoint 2: GET /api/v1/booking/vuelos/buscar
        /// </summary>
        public async Task<BookingVueloBuscarResultadoDto> BuscarDisponiblesBookingAsync(
            BookingVueloBuscarFiltrosDto filtros,
            CancellationToken cancellationToken = default)
        {
            var fechaInicio = filtros.Fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            var query = _context.Vuelos
                .AsNoTracking()
                .Where(v =>
                    v.IdAeropuertoOrigen == filtros.IdAeropuertoOrigen &&
                    v.IdAeropuertoDestino == filtros.IdAeropuertoDestino &&
                    v.FechaHoraSalida >= fechaInicio &&
                    v.FechaHoraSalida < fechaFin &&
                    v.EstadoVuelo == "PROGRAMADO" &&
                    !v.Eliminado);

            // Filtro por clase — solo vuelos que tengan asientos disponibles de esa clase
            if (!string.IsNullOrWhiteSpace(filtros.Clase))
                query = query.Where(v =>
                    v.Asientos.Any(a => a.Clase == filtros.Clase && a.Disponible && !a.Eliminado));

            // Filtro por número máximo de escalas (0 = solo directos)
            if (filtros.Escalas.HasValue)
                query = query.Where(v =>
                    v.Escalas.Count(e => !e.Eliminado) <= filtros.Escalas.Value);

            // Filtro por hora de salida desde
            if (filtros.HoraSalidaDesde.HasValue)
            {
                var h = filtros.HoraSalidaDesde.Value.Hour;
                var m = filtros.HoraSalidaDesde.Value.Minute;
                query = query.Where(v =>
                    v.FechaHoraSalida.Hour > h ||
                    (v.FechaHoraSalida.Hour == h && v.FechaHoraSalida.Minute >= m));
            }

            // Filtro por hora de salida hasta
            if (filtros.HoraSalidaHasta.HasValue)
            {
                var h = filtros.HoraSalidaHasta.Value.Hour;
                var m = filtros.HoraSalidaHasta.Value.Minute;
                query = query.Where(v =>
                    v.FechaHoraSalida.Hour < h ||
                    (v.FechaHoraSalida.Hour == h && v.FechaHoraSalida.Minute <= m));
            }

            // Filtro por hora de llegada desde
            if (filtros.HoraLlegadaDesde.HasValue)
            {
                var h = filtros.HoraLlegadaDesde.Value.Hour;
                var m = filtros.HoraLlegadaDesde.Value.Minute;
                query = query.Where(v =>
                    v.FechaHoraLlegada.Hour > h ||
                    (v.FechaHoraLlegada.Hour == h && v.FechaHoraLlegada.Minute >= m));
            }

            // Filtro por hora de llegada hasta
            if (filtros.HoraLlegadaHasta.HasValue)
            {
                var h = filtros.HoraLlegadaHasta.Value.Hour;
                var m = filtros.HoraLlegadaHasta.Value.Minute;
                query = query.Where(v =>
                    v.FechaHoraLlegada.Hour < h ||
                    (v.FechaHoraLlegada.Hour == h && v.FechaHoraLlegada.Minute <= m));
            }

            // Filtro por duración máxima total del vuelo
            if (filtros.DuracionMaxMin.HasValue)
                query = query.Where(v => v.DuracionMin <= filtros.DuracionMaxMin.Value);

            // Filtro por duración máxima de cada escala
            if (filtros.EscalaDuracionMax.HasValue)
                query = query.Where(v =>
                    v.Escalas.All(e => e.Eliminado || e.DuracionMin <= filtros.EscalaDuracionMax.Value));

            // Filtro por aeropuerto de escala específico
            if (filtros.IdAeropuertoEscala.HasValue)
                query = query.Where(v =>
                    v.Escalas.Any(e => e.IdAeropuerto == filtros.IdAeropuertoEscala.Value && !e.Eliminado));

            // Filtro por precio mínimo
            if (filtros.PrecioMin.HasValue)
                query = query.Where(v => v.PrecioBase >= filtros.PrecioMin.Value);

            // Filtro por precio máximo
            if (filtros.PrecioMax.HasValue)
                query = query.Where(v => v.PrecioBase <= filtros.PrecioMax.Value);

            // Total antes de paginar — para el campo meta.total del contrato
            var total = await query.CountAsync(cancellationToken);

            // Ordenamiento
            query = filtros.OrdenarPor?.ToUpper() switch
            {
                "PRECIO" => filtros.OrdenDir == "DESC"
                    ? query.OrderByDescending(v => v.PrecioBase)
                    : query.OrderBy(v => v.PrecioBase),
                "DURACION" => filtros.OrdenDir == "DESC"
                    ? query.OrderByDescending(v => v.DuracionMin)
                    : query.OrderBy(v => v.DuracionMin),
                "SALIDA" => filtros.OrdenDir == "DESC"
                    ? query.OrderByDescending(v => v.FechaHoraSalida)
                    : query.OrderBy(v => v.FechaHoraSalida),
                "LLEGADA" => filtros.OrdenDir == "DESC"
                    ? query.OrderByDescending(v => v.FechaHoraLlegada)
                    : query.OrderBy(v => v.FechaHoraLlegada),
                _ => query.OrderBy(v => v.PrecioBase)  // default: PRECIO ASC
            };

            // Paginación
            var skip = (filtros.Page - 1) * filtros.Limit;

            var vuelos = await query
                .Skip(skip)
                .Take(filtros.Limit)
                .Select(v => new BookingVueloBuscarDto
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    IdAeropuertoOrigen = v.IdAeropuertoOrigen,
                    IdAeropuertoDestino = v.IdAeropuertoDestino,
                    FechaHoraSalida = v.FechaHoraSalida,
                    FechaHoraLlegada = v.FechaHoraLlegada,
                    DuracionMin = v.DuracionMin,
                    NumEscalas = v.Escalas.Count(e => !e.Eliminado),
                    PrecioBase = v.PrecioBase,
                    AsientosDisponibles = v.Asientos.Count(a => a.Disponible && !a.Eliminado),
                    EstadoVuelo = v.EstadoVuelo
                })
                .ToListAsync(cancellationToken);

            return new BookingVueloBuscarResultadoDto
            {
                Total = total,
                Page = filtros.Page,
                Limit = filtros.Limit,
                Vuelos = vuelos
            };
        }

        /// <summary>
        /// Detalle completo de un vuelo con disponibilidad agrupada por clase.
        /// Endpoint 3: GET /api/v1/booking/vuelos/{id_vuelo}
        /// </summary>
        public async Task<BookingVueloDetalleCompletoDto?> ObtenerDetalleBookingAsync(
            int idVuelo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Vuelos
                .AsNoTracking()
                .Where(v => v.IdVuelo == idVuelo && !v.Eliminado)
                .Select(v => new BookingVueloDetalleCompletoDto
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    IdAeropuertoOrigen = v.IdAeropuertoOrigen,
                    IdAeropuertoDestino = v.IdAeropuertoDestino,
                    FechaHoraSalida = v.FechaHoraSalida,
                    FechaHoraLlegada = v.FechaHoraLlegada,
                    DuracionMin = v.DuracionMin,
                    PrecioBase = v.PrecioBase,
                    CapacidadTotal = v.CapacidadTotal,
                    EstadoVuelo = v.EstadoVuelo,
                    DisponibilidadPorClase = v.Asientos
                        .Where(a => !a.Eliminado && a.Estado == "ACTIVO")
                        .GroupBy(a => a.Clase)
                        .Select(g => new BookingDisponibilidadClaseDto
                        {
                            Clase = g.Key,
                            AsientosDisponibles = g.Count(a => a.Disponible),
                            PrecioBase = v.PrecioBase
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Escalas completas de un vuelo con todos los campos del contrato.
        /// Endpoint 4: GET /api/v1/booking/vuelos/{id_vuelo}/escalas
        /// </summary>
        public async Task<BookingVueloEscalasDto?> ObtenerEscalasBookingAsync(
            int idVuelo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Vuelos
                .AsNoTracking()
                .Where(v => v.IdVuelo == idVuelo && !v.Eliminado)
                .Select(v => new BookingVueloEscalasDto
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    NumEscalas = v.Escalas.Count(e => !e.Eliminado),
                    Escalas = v.Escalas
                        .Where(e => !e.Eliminado)
                        .OrderBy(e => e.Orden)
                        .Select(e => new BookingEscalaDetalleDto
                        {
                            IdEscala = e.IdEscala,
                            Orden = e.Orden,
                            IdAeropuerto = e.IdAeropuerto,
                            FechaHoraLlegada = e.FechaHoraLlegada,
                            FechaHoraSalida = e.FechaHoraSalida,
                            DuracionMin = e.DuracionMin,
                            TipoEscala = e.TipoEscala,
                            Terminal = e.Terminal,
                            Puerta = e.Puerta,
                            Observaciones = e.Observaciones
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}