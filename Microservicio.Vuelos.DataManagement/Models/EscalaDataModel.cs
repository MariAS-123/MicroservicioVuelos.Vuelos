namespace Microservicio.Vuelos.DataManagement.Models;

/// <summary>
/// POCO independiente de EF Core que representa una escala.
/// CAMBIO MICROSERVICIO: se eliminó RowVersion — era específico de SQL Server.
/// IdAeropuerto es referencia lógica al MS Aeropuertos — solo INT, sin navegación.
/// </summary>
public class EscalaDataModel
{
    public int IdEscala { get; set; }

    // FK interna — vuelo está en la misma BDD
    public int IdVuelo { get; set; }

    // Referencia lógica al MS Aeropuertos — solo ID
    public int IdAeropuerto { get; set; }

    public int Orden { get; set; }

    public DateTime FechaHoraLlegada { get; set; }

    public DateTime FechaHoraSalida { get; set; }

    // Calculado por el backend
    public int DuracionMin { get; set; }

    // TECNICA | COMERCIAL
    public string TipoEscala { get; set; } = null!;

    public string? Terminal { get; set; }

    public string? Puerta { get; set; }

    public string? Observaciones { get; set; }

    public string Estado { get; set; } = null!;

    public bool Eliminado { get; set; }

    public DateTime FechaRegistroUtc { get; set; }

    public string CreadoPorUsuario { get; set; } = null!;

    public string? ModificadoPorUsuario { get; set; }

    public DateTime? FechaModificacionUtc { get; set; }

    public string? ModificacionIp { get; set; }
}