namespace Microservicio.Vuelos.DataManagement.Models;

/// <summary>
/// Modelo de filtros para búsquedas paginadas de vuelos.
/// Sin cambios respecto al monolito.
/// </summary>
public class VueloFiltroDataModel
{
    public string? NumeroVuelo { get; set; }

    public int? IdAeropuertoOrigen { get; set; }

    public int? IdAeropuertoDestino { get; set; }

    // PROGRAMADO | EN_VUELO | ATERRIZADO | CANCELADO | DEMORADO
    public string? EstadoVuelo { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaSalidaDesde { get; set; }

    public DateTime? FechaSalidaHasta { get; set; }

    public bool IncluirEliminados { get; set; } = false;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}