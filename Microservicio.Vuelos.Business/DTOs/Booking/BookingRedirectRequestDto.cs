using System.ComponentModel.DataAnnotations;

namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingRedirectRequestDto
{
    [Required]
    public int IdVueloIda { get; set; }

    public int? IdVueloRetorno { get; set; }

    [Required]
    public string Clase { get; set; } = string.Empty;

    [Required]
    [Range(1, 9)]
    public int Pasajeros { get; set; }

    public string? ReferenciaBooking { get; set; }
}