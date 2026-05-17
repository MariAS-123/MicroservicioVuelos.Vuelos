namespace Microservicio.Vuelos.Business.Integrations;

/// <summary>
/// DTO con los campos del MS Aeropuertos que MS Vuelos necesita.
/// Ampliado para soportar los endpoints del Booking que requieren
/// datos completos de aeropuerto, ciudad y país.
/// </summary>
public class AeropuertoIntegrationDto
{
    public int IdAeropuerto { get; set; }
    public string CodigoIata { get; set; } = null!;
    public string? CodigoIcao { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Estado { get; set; }
    public string? ZonaHoraria { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public int? IdCiudad { get; set; }        // ← agregar
    public int? IdPais { get; set; }          // ← agregar
    public AeropuertoCiudadIntegrationDto? Ciudad { get; set; }  // ← nullable
    public AeropuertoPaisIntegrationDto? Pais { get; set; }      // ← nullable
}

public class AeropuertoCiudadIntegrationDto
{
    public int IdCiudad { get; set; }
    public string Nombre { get; set; } = null!;
    public string ZonaHoraria { get; set; } = null!;
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}

public class AeropuertoPaisIntegrationDto
{
    public int IdPais { get; set; }
    public string CodigoIso2 { get; set; } = null!;
    public string CodigoIso3 { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Continente { get; set; }
}