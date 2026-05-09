namespace Microservicio.Vuelos.DataManagement.Models;

/// <summary>
/// Modelo de filtros para búsquedas paginadas de escalas.
/// Sin cambios respecto al monolito.
/// </summary>
public class EscalaFiltroDataModel
{
    public int? IdVuelo { get; set; }

    // Referencia lógica al MS Aeropuertos
    public int? IdAeropuerto { get; set; }

    public int? Orden { get; set; }

    // TECNICA | COMERCIAL
    public string? TipoEscala { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaLlegadaDesde { get; set; }

    public DateTime? FechaLlegadaHasta { get; set; }

    public DateTime? FechaSalidaDesde { get; set; }

    public DateTime? FechaSalidaHasta { get; set; }

    public bool IncluirEliminados { get; set; } = false;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}