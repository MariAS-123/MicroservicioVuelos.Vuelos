namespace Microservicio.Vuelos.Business.DTOs.Booking;

public class BookingVueloDetalleResponseDto
{
    public int IdVuelo { get; set; }
    public string NumeroVuelo { get; set; } = string.Empty;
    public BookingAeropuertoDetalleDto Origen { get; set; } = null!;
    public BookingAeropuertoDetalleDto Destino { get; set; } = null!;
    public DateTime FechaHoraSalida { get; set; }
    public DateTime FechaHoraLlegada { get; set; }
    public int DuracionMin { get; set; }
    public decimal PrecioBase { get; set; }
    public int CapacidadTotal { get; set; }
    public string EstadoVuelo { get; set; } = string.Empty;
    public List<BookingDisponibilidadClaseDto> DisponibilidadPorClase { get; set; } = new();
}

public class BookingAeropuertoDetalleDto
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
}

public class BookingCiudadDto
{
    public int IdCiudad { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = string.Empty;
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}

public class BookingPaisDto
{
    public int IdPais { get; set; }
    public string CodigoIso2 { get; set; } = string.Empty;
    public string CodigoIso3 { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Continente { get; set; }
}

public class BookingDisponibilidadClaseDto
{
    public string Clase { get; set; } = string.Empty;
    public int AsientosDisponibles { get; set; }
    public decimal PrecioBase { get; set; }
}