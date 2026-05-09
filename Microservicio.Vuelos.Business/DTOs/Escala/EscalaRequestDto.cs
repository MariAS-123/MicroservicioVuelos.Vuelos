// ===== EscalaRequestDto.cs =====
namespace Microservicio.Vuelos.Business.DTOs.Escala;

public class EscalaRequestDto
{
    public int IdVuelo { get; set; }
    public int IdAeropuerto { get; set; }
    public int Orden { get; set; }
    public DateTime FechaHoraLlegada { get; set; }
    public DateTime FechaHoraSalida { get; set; }
    public int DuracionMin { get; set; }
    public string TipoEscala { get; set; } = null!;
    public string? Terminal { get; set; }
    public string? Puerta { get; set; }
    public string? Observaciones { get; set; }
    // Estado se asigna ACTIVO automáticamente
}