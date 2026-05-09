using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Vuelos.Business.DTOs.Vuelo;

public class VueloFilterDto
{
    [FromQuery(Name = "id_aeropuerto_origen")]
    public int? IdAeropuertoOrigen { get; set; }

    [FromQuery(Name = "id_aeropuerto_destino")]
    public int? IdAeropuertoDestino { get; set; }

    [FromQuery(Name = "fecha_salida")]
    public DateTime? FechaSalida { get; set; }

    [FromQuery(Name = "numero_vuelo")]
    public string? NumeroVuelo { get; set; }

    [FromQuery(Name = "estado_vuelo")]
    public string? EstadoVuelo { get; set; }

    [FromQuery(Name = "estado")]
    public string? Estado { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "page_size")]
    public int PageSize { get; set; } = 20;
}