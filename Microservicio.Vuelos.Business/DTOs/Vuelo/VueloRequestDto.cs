namespace Microservicio.Vuelos.Business.DTOs.Vuelo;

public class VueloRequestDto
{
    public int IdAeropuertoOrigen { get; set; }
    public int IdAeropuertoDestino { get; set; }
    public string NumeroVuelo { get; set; } = null!;
    public DateTime FechaHoraSalida { get; set; }
    public DateTime FechaHoraLlegada { get; set; }
    public int DuracionMin { get; set; }
    public decimal PrecioBase { get; set; }
    public int CapacidadTotal { get; set; }
    // Estado=ACTIVO automático, EstadoVuelo=PROGRAMADO automático
}