using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingRedirectRequestDto
{
    [JsonPropertyName("idVuelo")]
    [Required]
    public int IdVuelo { get; set; }

    [JsonPropertyName("idAsientos")]
    public List<int> IdAsientos { get; set; } = [];

    [JsonPropertyName("urlRetorno")]
    [Required]
    public string UrlRetorno { get; set; } = string.Empty;
}