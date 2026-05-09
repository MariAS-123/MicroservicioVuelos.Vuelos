using Microsoft.EntityFrameworkCore;
using Microservicio.Vuelos.DataAccess.Context;

namespace Microservicio.Vuelos.DataAccess.Queries
{
    /// <summary>
    /// Consultas especializadas de asientos (mapa de cabina, disponibilidad, ocupación).
    /// CAMBIOS MICROSERVICIO:
    ///
    ///   ObtenerMapaPorVueloAsync:
    ///     - Se eliminó Pasajero = a.ReservaDetalles.Select(d => d.Pasajero.Nombre...)
    ///       porque ReservaDetalleEntity y PasajeroEntity pertenecen a MS Ventas
    ///       y MS Clientes respectivamente. No existen en BDD_Vuelos.
    ///     - El campo Pasajero se elimina del DTO. Si se necesita mostrar el nombre
    ///       del pasajero en el mapa, MS Ventas es quien debe enriquecerlo al consultar
    ///       sus propias reservas.
    ///
    ///   ObtenerDisponiblesPorClaseAsync: sin cambios — solo usa vuelos.asiento.
    ///   ObtenerResumenOcupacionAsync: sin cambios — solo usa vuelos.vuelo y asientos.
    /// </summary>
    public class AsientoQueryRepository
    {
        private readonly VuelosDbContext _context;

        public AsientoQueryRepository(VuelosDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DTOs internos del QueryRepository
        // ─────────────────────────────────────────────────────────────────────

        public class AsientoMapaDto
        {
            public int IdAsiento { get; set; }
            public string NumeroAsiento { get; set; } = string.Empty;
            public string Clase { get; set; } = string.Empty;
            public string? Posicion { get; set; }
            public bool Disponible { get; set; }
            public decimal PrecioExtra { get; set; }
            // CAMBIO: el campo Pasajero fue eliminado.
            // En el monolito se obtenía via ReservaDetalles → Pasajero,
            // entidades de otros MS que no existen en BDD_Vuelos.
            // MS Ventas es responsable de cruzar asiento + pasajero.
        }

        public class ResumenOcupacionDto
        {
            public int IdVuelo { get; set; }
            public int TotalAsientos { get; set; }
            public int Disponibles { get; set; }
            public int Ocupados { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Consultas
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Retorna el mapa visual de la cabina para un vuelo dado.
        /// Solo incluye datos de vuelos.asiento — disponibilidad y clase.
        /// El nombre del pasajero asignado NO se incluye (ver comentario del archivo).
        /// </summary>
        public async Task<List<AsientoMapaDto>> ObtenerMapaPorVueloAsync(
            int idVuelo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Asientos
                .AsNoTracking()
                .Where(a => a.IdVuelo == idVuelo && !a.Eliminado)
                .OrderBy(a => a.NumeroAsiento)
                .Select(a => new AsientoMapaDto
                {
                    IdAsiento = a.IdAsiento,
                    NumeroAsiento = a.NumeroAsiento,
                    Clase = a.Clase,
                    Posicion = a.Posicion,
                    Disponible = a.Disponible,
                    PrecioExtra = a.PrecioExtra
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retorna asientos disponibles de una clase específica para un vuelo.
        /// Sin cambios de lógica respecto al monolito.
        /// </summary>
        public async Task<List<AsientoMapaDto>> ObtenerDisponiblesPorClaseAsync(
            int idVuelo,
            string clase,
            CancellationToken cancellationToken = default)
        {
            return await _context.Asientos
                .AsNoTracking()
                .Where(a => a.IdVuelo == idVuelo && a.Clase == clase && a.Disponible && !a.Eliminado)
                .OrderBy(a => a.NumeroAsiento)
                .Select(a => new AsientoMapaDto
                {
                    IdAsiento = a.IdAsiento,
                    NumeroAsiento = a.NumeroAsiento,
                    Clase = a.Clase,
                    Posicion = a.Posicion,
                    Disponible = a.Disponible,
                    PrecioExtra = a.PrecioExtra
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retorna el resumen de ocupación de un vuelo: total, disponibles y ocupados.
        /// Sin cambios de lógica respecto al monolito.
        /// </summary>
        public async Task<ResumenOcupacionDto?> ObtenerResumenOcupacionAsync(
            int idVuelo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Vuelos
                .AsNoTracking()
                .Where(v => v.IdVuelo == idVuelo && !v.Eliminado)
                .Select(v => new ResumenOcupacionDto
                {
                    IdVuelo = v.IdVuelo,
                    TotalAsientos = v.Asientos.Count(a => !a.Eliminado),
                    Disponibles = v.Asientos.Count(a => a.Disponible && !a.Eliminado),
                    Ocupados = v.Asientos.Count(a => !a.Disponible && !a.Eliminado)
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}