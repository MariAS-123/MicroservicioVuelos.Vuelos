namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingAeropuertoResponseDto
{
    public int IdAeropuerto { get; set; }
    public string CodigoIata { get; set; } = string.Empty;
    public string CodigoIcao { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public BookingCiudadDto Ciudad { get; set; } = null!;
    public BookingPaisDto Pais { get; set; } = null!;
    public string ZonaHoraria { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string Estado { get; set; } = string.Empty;
}