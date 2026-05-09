namespace Microservicio.Vuelos.Business.DTOs.Vuelo;

public class VueloEstadoRequestDto
{
    // PROGRAMADO | EN_VUELO | ATERRIZADO | CANCELADO | DEMORADO
    public string EstadoVuelo { get; set; } = null!;
}