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
    /// </summary>
    public class VueloQueryRepository
    {
        private readonly VuelosDbContext _context;

        public VueloQueryRepository(VuelosDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DTOs internos del QueryRepository
        // ─────────────────────────────────────────────────────────────────────

        public class VueloDisponibleDto
        {
            public int IdVuelo { get; set; }
            public string NumeroVuelo { get; set; } = string.Empty;
            public DateTime FechaHoraSalida { get; set; }
            public DateTime FechaHoraLlegada { get; set; }
            public decimal PrecioBase { get; set; }
            // CAMBIO: antes era string con el nombre del aeropuerto obtenido por
            // navegación EF. Ahora retorna el ID — la capa Business llama al
            // MS Aeropuertos si necesita el nombre para el ResponseDto.
            public int IdAeropuertoOrigen { get; set; }
            public int IdAeropuertoDestino { get; set; }
            public int AsientosDisponibles { get; set; }
        }

        public class EscalaDto
        {
            public int Orden { get; set; }
            // CAMBIO: antes era string Aeropuerto con nombre via navegación EF.
            // Ahora retorna el ID para que Business lo enriquezca si hace falta.
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
            // CAMBIO: IDs en lugar de nombres — ver comentario en VueloDisponibleDto
            public int IdAeropuertoOrigen { get; set; }
            public int IdAeropuertoDestino { get; set; }
            public int AsientosDisponibles { get; set; }
            // CAMBIO: BoletosEmitidos eliminado — BoletoEntity no existe en esta BDD
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
        // Consultas
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
                    // BoletosEmitidos eliminado — BoletoEntity no existe en BDD_Vuelos
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
            // Sin cambios — solo usa vuelos.vuelo y vuelos.asiento (misma BDD)
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
    }
}