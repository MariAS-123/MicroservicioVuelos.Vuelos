namespace Microservicio.Vuelos.DataManagement.Models;

/// <summary>
/// POCO independiente de EF Core que representa un vuelo.
/// CAMBIO MICROSERVICIO: se eliminó RowVersion — era específico de SQL Server.
/// PostgreSQL maneja concurrencia con xmin, no se mapea en los DataModels.
/// </summary>
public class VueloDataModel
{
    public int IdVuelo { get; set; }

    // Referencias lógicas al MS Aeropuertos — solo IDs, sin navegación
    public int IdAeropuertoOrigen { get; set; }

    public int IdAeropuertoDestino { get; set; }

    public string NumeroVuelo { get; set; } = null!;

    public DateTime FechaHoraSalida { get; set; }

    public DateTime FechaHoraLlegada { get; set; }

    // Calculado por el backend
    public int DuracionMin { get; set; }

    public decimal PrecioBase { get; set; }

    public int CapacidadTotal { get; set; }

    // PROGRAMADO | EN_VUELO | ATERRIZADO | CANCELADO | DEMORADO
    public string EstadoVuelo { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public bool Eliminado { get; set; }

    public DateTime FechaRegistroUtc { get; set; }

    public string CreadoPorUsuario { get; set; } = null!;

    public string? ModificadoPorUsuario { get; set; }

    public DateTime? FechaModificacionUtc { get; set; }

    public string? ModificacionIp { get; set; }
}