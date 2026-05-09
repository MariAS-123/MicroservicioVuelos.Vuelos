using Microservicio.Vuelos.DataAccess.Entities;
using System;
using System.Collections.Generic;

namespace Microservicio.Vuelos.DataAccess.Entities
{
    /// <summary>
    /// Entidad que representa la tabla vuelos.vuelo en BDD_Vuelos.
    /// NOTA MICROSERVICIO: AeropuertoOrigen y AeropuertoDestino son referencias
    /// lógicas al MS Aeropuertos. No existen como navegaciones EF aquí.
    /// ReservaEntity y BoletoEntity pertenecen a otros MS — eliminadas.
    /// </summary>
    public class VueloEntity
    {
        public int IdVuelo { get; set; }

        // Referencia lógica al MS Aeropuertos — solo se guarda el INT, sin FK física
        public int IdAeropuertoOrigen { get; set; }

        // Referencia lógica al MS Aeropuertos — solo se guarda el INT, sin FK física
        public int IdAeropuertoDestino { get; set; }

        public string NumeroVuelo { get; set; } = null!;

        public DateTime FechaHoraSalida { get; set; }

        public DateTime FechaHoraLlegada { get; set; }

        // Calculado por el backend: no lo envía el cliente
        public int DuracionMin { get; set; }

        public decimal PrecioBase { get; set; }

        public int CapacidadTotal { get; set; }

        // PROGRAMADO | EN_VUELO | ATERRIZADO | CANCELADO | DEMORADO
        public string EstadoVuelo { get; set; } = null!;

        public string Estado { get; set; } = null!;

        public bool Eliminado { get; set; }

        public DateTime FechaRegistroUtc { get; set; }

        public string CreadoPorUsuario { get; set; } = null!;

        public string? ModificadoPorUsuario { get; set; }

        public DateTime? FechaModificacionUtc { get; set; }

        public string? ModificacionIp { get; set; }

        // Navegaciones INTERNAS a esta misma BDD_Vuelos — se mantienen
        public virtual ICollection<EscalaEntity> Escalas { get; set; } = new HashSet<EscalaEntity>();

        public virtual ICollection<AsientoEntity> Asientos { get; set; } = new HashSet<AsientoEntity>();
    }
}