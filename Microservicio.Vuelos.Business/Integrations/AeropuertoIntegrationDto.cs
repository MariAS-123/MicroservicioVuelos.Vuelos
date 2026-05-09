namespace Microservicio.Vuelos.Business.Integrations;

/// <summary>
/// DTO con los campos mínimos que MS Vuelos necesita del MS Aeropuertos.
/// No replica todos los campos del aeropuerto — solo los necesarios para
/// validar existencia y enriquecer respuestas.
/// </summary>
public class AeropuertoIntegrationDto
{
    public int IdAeropuerto { get; set; }
    public string CodigoIata { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Estado { get; set; } = null!;
}