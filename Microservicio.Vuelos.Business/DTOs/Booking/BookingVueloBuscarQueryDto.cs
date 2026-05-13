namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingVueloBuscarQueryDto
{
    // Obligatorios
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }

    // Opcionales
    public string? Clase { get; set; }
    public int? Escalas { get; set; }
    public TimeOnly? HoraSalidaDesde { get; set; }
    public TimeOnly? HoraSalidaHasta { get; set; }
    public TimeOnly? HoraLlegadaDesde { get; set; }
    public TimeOnly? HoraLlegadaHasta { get; set; }
    public int? DuracionMaxMin { get; set; }
    public int? EscalaDuracionMax { get; set; }
    public string? AeropuertoEscala { get; set; }   // codigo_iata — Business lo resuelve a ID
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }

    // Ordenamiento
    public string? OrdenarPor { get; set; }          // PRECIO · DURACION · SALIDA · LLEGADA
    public string OrdenDir { get; set; } = "ASC";

    // Paginación
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}