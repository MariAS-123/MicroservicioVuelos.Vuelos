namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingAsientoResponseDto
{
    public int IdVuelo { get; set; }
    public string NumeroVuelo { get; set; } = string.Empty;
    public BookingAsientoResumenDto Resumen { get; set; } = null!;
    public List<BookingAsientoItemDto> Asientos { get; set; } = new();
}

public class BookingAsientoResumenDto
{
    public int TotalAsientos { get; set; }
    public int Disponibles { get; set; }
    public int Ocupados { get; set; }
}

public class BookingAsientoItemDto
{
    public int IdAsiento { get; set; }
    public string NumeroAsiento { get; set; } = string.Empty;
    public string Clase { get; set; } = string.Empty;
    public bool Disponible { get; set; }
    public decimal PrecioExtra { get; set; }
    public string? Posicion { get; set; }
}