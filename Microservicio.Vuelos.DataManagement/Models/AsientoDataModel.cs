namespace Microservicio.Vuelos.DataManagement.Models;

/// <summary>
/// POCO independiente de EF Core que representa un asiento.
/// CAMBIO MICROSERVICIO: se eliminó RowVersion — era específico de SQL Server.
/// </summary>
public class AsientoDataModel
{
    public int IdAsiento { get; set; }

    // FK interna — vuelo está en la misma BDD
    public int IdVuelo { get; set; }

    public string NumeroAsiento { get; set; } = null!;

    // ECONOMICA | EJECUTIVA | PRIMERA
    public string Clase { get; set; } = null!;

    public bool Disponible { get; set; }

    public decimal PrecioExtra { get; set; }

    // VENTANA | PASILLO | CENTRO — nullable
    public string? Posicion { get; set; }

    public string Estado { get; set; } = null!;

    public bool Eliminado { get; set; }

    public DateTime FechaRegistroUtc { get; set; }

    public string CreadoPorUsuario { get; set; } = null!;

    public string? ModificadoPorUsuario { get; set; }

    public DateTime? FechaModificacionUtc { get; set; }

    public string? ModificacionIp { get; set; }
}