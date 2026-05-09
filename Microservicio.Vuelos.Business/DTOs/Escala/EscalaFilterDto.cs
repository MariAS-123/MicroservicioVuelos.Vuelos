namespace Microservicio.Vuelos.Business.DTOs.Escala;

public class EscalaFilterDto
{
    public int? IdVuelo { get; set; }
    public int? IdAeropuerto { get; set; }
    public int? Orden { get; set; }
    public string? TipoEscala { get; set; }
    public string? Estado { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}