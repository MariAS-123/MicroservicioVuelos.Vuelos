using System;

namespace Microservicio.Vuelos.DataAccess.Entities
{
    /// <summary>
    /// Entidad que representa la tabla vuelos.escala en BDD_Vuelos.
    /// NOTA MICROSERVICIO: IdAeropuerto es referencia lógica al MS Aeropuertos.
    /// No existe navegación EF a AeropuertoEntity — pertenece a otra BDD.
    /// La FK física con vuelos.vuelo SÍ se mantiene (misma BDD).
    /// </summary>
    public class EscalaEntity
    {
        public int IdEscala { get; set; }

        // FK interna — vuelos.vuelo está en esta misma BDD
        public int IdVuelo { get; set; }

        // Referencia lógica al MS Aeropuertos — solo INT, sin FK física ni navegación
        public int IdAeropuerto { get; set; }

        public int Orden { get; set; }

        public DateTime FechaHoraLlegada { get; set; }

        public DateTime FechaHoraSalida { get; set; }

        // Calculado por el backend: FechaHoraSalida - FechaHoraLlegada
        public int DuracionMin { get; set; }

        // TECNICA | COMERCIAL
        public string TipoEscala { get; set; } = null!;

        public string? Terminal { get; set; }

        public string? Puerta { get; set; }

        public string? Observaciones { get; set; }

        public string Estado { get; set; } = null!;

        public bool Eliminado { get; set; }

        public DateTime FechaRegistroUtc { get; set; }

        public string CreadoPorUsuario { get; set; } = null!;

        public string? ModificadoPorUsuario { get; set; }

        public DateTime? FechaModificacionUtc { get; set; }

        public string? ModificacionIp { get; set; }

        // Navegación interna a VueloEntity — misma BDD, se mantiene
        public virtual VueloEntity Vuelo { get; set; } = null!;
    }
}