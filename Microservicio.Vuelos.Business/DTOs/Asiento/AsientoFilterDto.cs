using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Vuelos.Business.DTOs.Asiento;

public class AsientoFilterDto
{
    [FromQuery(Name = "id_vuelo")]
    public int? IdVuelo { get; set; }

    [FromQuery(Name = "disponible")]
    public bool? Disponible { get; set; }

    [FromQuery(Name = "clase")]
    public string? Clase { get; set; }

    [FromQuery(Name = "numero_asiento")]
    public string? NumeroAsiento { get; set; }

    [FromQuery(Name = "posicion")]
    public string? Posicion { get; set; }

    [FromQuery(Name = "estado")]
    public string? Estado { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "page_size")]
    public int PageSize { get; set; } = 20;
}