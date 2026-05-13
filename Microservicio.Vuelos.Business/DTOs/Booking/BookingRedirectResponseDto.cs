namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingRedirectResponseDto
{
    public string RedirectToken { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } = 900;          // 15 minutos — fijo según contrato
    public string TipoViaje { get; set; } = string.Empty;  // IDA · IDA_VUELTA
    public BookingRedirectContextoDto Contexto { get; set; } = null!;
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