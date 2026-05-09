using System;

namespace Microservicio.Vuelos.DataAccess.Entities
{
    public class AsientoEntity
    {
        public int IdAsiento { get; set; }
        public int IdVuelo { get; set; }
        public string NumeroAsiento { get; set; } = null!;
        public string Clase { get; set; } = null!;
        public bool Disponible { get; set; }
        public decimal PrecioExtra { get; set; }
        public string? Posicion { get; set; }
        public string Estado { get; set; } = null!;
        public bool Eliminado { get; set; }
        public DateTime FechaRegistroUtc { get; set; }
        public string CreadoPorUsuario { get; set; } = null!;
        public string? ModificadoPorUsuario { get; set; }
        public DateTime? FechaModificacionUtc { get; set; }
        public string? ModificacionIp { get; set; }
        public virtual VueloEntity Vuelo { get; set; } = null!;
    }
}