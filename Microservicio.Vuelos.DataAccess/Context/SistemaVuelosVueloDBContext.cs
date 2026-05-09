using Microservicio.Vuelos.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Vuelos.DataAccess.Context
{
    /// <summary>
    /// DbContext exclusivo del MS Vuelos.
    /// CAMBIOS MICROSERVICIO: El monolito tenía UN solo contexto con las 17 tablas
    /// de todos los dominios. Aquí solo existen las 3 tablas de BDD_Vuelos:
    ///   - vuelos.vuelo
    ///   - vuelos.escala
    ///   - vuelos.asiento
    /// PaisEntity, CiudadEntity, AeropuertoEntity → MS Geografía y MS Aeropuertos.
    /// ClienteEntity, UsuarioAppEntity, RolEntity → MS Seguridad y MS Clientes.
    /// ReservaEntity, FacturaEntity, BoletoEntity → MS Ventas y MS Facturación.
    /// </summary>
    public class VuelosDbContext : DbContext
    {
        public VuelosDbContext(DbContextOptions<VuelosDbContext> options)
            : base(options)
        {
        }

        // Solo las 3 tablas de este microservicio
        public DbSet<VueloEntity> Vuelos => Set<VueloEntity>();
        public DbSet<EscalaEntity> Escalas => Set<EscalaEntity>();
        public DbSet<AsientoEntity> Asientos => Set<AsientoEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aplica automáticamente VueloConfiguration, EscalaConfiguration
            // y AsientoConfiguration desde el mismo ensamblado
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VuelosDbContext).Assembly);
        }
    }
}