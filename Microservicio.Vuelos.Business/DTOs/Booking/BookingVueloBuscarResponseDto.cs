namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingVueloBuscarResponseDto
{
    public BookingVueloBuscarMetaDto Meta { get; set; } = null!;
    public List<BookingVueloItemDto> Data { get; set; } = new();
}

public class BookingVueloBuscarMetaDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public string Moneda { get; set; } = "USD";
}

public class BookingVueloItemDto
{
    public int IdVuelo { get; set; }
    public string NumeroVuelo { get; set; } = string.Empty;
    public BookingAeropuertoCortoDto Origen { get; set; } = null!;
    public BookingAeropuertoCortoDto Destino { get; set; } = null!;
    public DateTime FechaHoraSalida { get; set; }
    public DateTime FechaHoraLlegada { get; set; }
    public int DuracionMin { get; set; }
    public int NumEscalas { get; set; }
    public string? ClaseSolicitada { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal PrecioTotal { get; set; }         // precio_base * pasajeros si aplica
    public int AsientosDisponibles { get; set; }
    public string EstadoVuelo { get; set; } = string.Empty;
}

public class BookingAeropuertoCortoDto
{
    public int IdAeropuerto { get; set; }
    public string CodigoIata { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
}