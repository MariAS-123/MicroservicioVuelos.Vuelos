namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingEscalaResponseDto
{
    public int IdVuelo { get; set; }
    public string NumeroVuelo { get; set; } = string.Empty;
    public int NumEscalas { get; set; }
    public List<BookingEscalaItemDto> Escalas { get; set; } = new();
}

public class BookingEscalaItemDto
{
    public int IdEscala { get; set; }
    public int Orden { get; set; }
    public BookingAeropuertoEscalaDto Aeropuerto { get; set; } = null!;
    public DateTime FechaHoraLlegada { get; set; }
    public DateTime FechaHoraSalida { get; set; }
    public int DuracionMin { get; set; }
    public string TipoEscala { get; set; } = string.Empty;
    public string? Terminal { get; set; }
    public string? Puerta { get; set; }
    public string? Observaciones { get; set; }
}

public class BookingAeropuertoEscalaDto
{
    public int IdAeropuerto { get; set; }
    public string CodigoIata { get; set; } = string.Empty;
    public string CodigoIcao { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
}