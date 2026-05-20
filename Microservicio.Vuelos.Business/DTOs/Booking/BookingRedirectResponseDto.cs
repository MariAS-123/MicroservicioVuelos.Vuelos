using System.Text.Json.Serialization;

namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingRedirectResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("urlRedirect")]
    public string UrlRedirect { get; set; } = string.Empty;

    [JsonPropertyName("expiracion")]
    public DateTime Expiracion { get; set; }
}   

public class BookingRedirectContextoDto
{
    public BookingRedirectVueloDto Ida { get; set; } = null!;
    public BookingRedirectVueloDto? Retorno { get; set; }  // null si solo ida
}

public class BookingRedirectVueloDto
{
    public int IdVuelo { get; set; }
    public string NumeroVuelo { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public DateTime FechaHoraSalida { get; set; }
    public DateTime FechaHoraLlegada { get; set; }
    public int DuracionMin { get; set; }
    public decimal PrecioBaseReferencial { get; set; }
    public decimal PrecioTotalReferencial { get; set; }
}