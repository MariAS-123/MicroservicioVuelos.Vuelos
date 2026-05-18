using System.Text.Json.Serialization;

namespace Microservicio.Vuelos.Business.Integrations;

public class AeropuertoIntegrationDto
{
    [JsonPropertyName("idAeropuerto")]
    public int IdAeropuerto { get; set; }

    [JsonPropertyName("codigoIata")]
    public string CodigoIata { get; set; } = null!;

    [JsonPropertyName("codigoIcao")]
    public string? CodigoIcao { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = null!;

    [JsonPropertyName("estado")]
    public string? Estado { get; set; }

    [JsonPropertyName("zonaHoraria")]
    public string? ZonaHoraria { get; set; }

    [JsonPropertyName("latitud")]
    public decimal? Latitud { get; set; }

    [JsonPropertyName("longitud")]
    public decimal? Longitud { get; set; }

    // MS Aeropuertos admin devuelve integers directos
    [JsonPropertyName("idCiudad")]
    public int? IdCiudad { get; set; }

    [JsonPropertyName("idPais")]
    public int? IdPais { get; set; }

    // MS Aeropuertos booking devuelve objetos anidados (pueden ser null)
    [JsonPropertyName("ciudad")]
    public AeropuertoCiudadIntegrationDto? Ciudad { get; set; }

    [JsonPropertyName("pais")]
    public AeropuertoPaisIntegrationDto? Pais { get; set; }
}

public class AeropuertoCiudadIntegrationDto
{
    [JsonPropertyName("idCiudad")]
    public int IdCiudad { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = null!;

    [JsonPropertyName("zonaHoraria")]
    public string? ZonaHoraria { get; set; }

    [JsonPropertyName("latitud")]
    public decimal? Latitud { get; set; }

    [JsonPropertyName("longitud")]
    public decimal? Longitud { get; set; }
}

public class AeropuertoPaisIntegrationDto
{
    [JsonPropertyName("idPais")]
    public int IdPais { get; set; }

    [JsonPropertyName("codigoIso2")]
    public string? CodigoIso2 { get; set; }

    [JsonPropertyName("codigoIso3")]
    public string? CodigoIso3 { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = null!;

    [JsonPropertyName("continente")]
    public string? Continente { get; set; }
}