namespace Microservicio.Vuelos.DataManagement.Models;

/// <summary>
/// Modelo de filtros para búsquedas paginadas de asientos.
/// Sin cambios respecto al monolito.
/// </summary>
public class AsientoFiltroDataModel
{
    public int? IdVuelo { get; set; }

    public string? NumeroAsiento { get; set; }

    // ECONOMICA | EJECUTIVA | PRIMERA
    public string? Clase { get; set; }

    public bool? Disponible { get; set; }

    // VENTANA | PASILLO | CENTRO
    public string? Posicion { get; set; }

    public string? Estado { get; set; }

    public decimal? PrecioExtraDesde { get; set; }

    public decimal? PrecioExtraHasta { get; set; }

    public bool IncluirEliminados { get; set; } = false;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}