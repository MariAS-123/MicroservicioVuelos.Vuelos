using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microservicio.Vuelos.DataAccess.Entities;

namespace Microservicio.Vuelos.DataAccess.Configurations
{
    /// <summary>
    /// Configuración Fluent API para vuelos.vuelo.
    /// CAMBIOS MICROSERVICIO:
    ///   - Se eliminaron HasOne(AeropuertoOrigen) y HasOne(AeropuertoDestino)
    ///     porque AeropuertoEntity no existe en BDD_Vuelos.
    ///   - IdAeropuertoOrigen e IdAeropuertoDestino son columnas INT normales
    ///     sin CONSTRAINT FOREIGN KEY. La validación la hace VueloService
    ///     llamando al MS Aeropuertos vía HTTP.
    ///   - Se eliminó RowVersion (SQL Server) — PostgreSQL usa xmin para
    ///     concurrencia optimista, no se mapea en EF de la misma forma.
    /// </summary>
    public class VueloConfiguration : IEntityTypeConfiguration<VueloEntity>
    {
        public void Configure(EntityTypeBuilder<VueloEntity> builder)
        {
            builder.ToTable("vuelo", "vuelos");

            builder.HasKey(e => e.IdVuelo)
                .HasName("pk_vuelo");

            builder.Property(e => e.IdVuelo)
                .HasColumnName("id_vuelo")
                .UseIdentityAlwaysColumn(); // PostgreSQL SERIAL equivalente en EF

            builder.Property(e => e.IdAeropuertoOrigen)
                .HasColumnName("id_aeropuerto_origen")
                .IsRequired();
            // Sin HasForeignKey — es referencia lógica al MS Aeropuertos

            builder.Property(e => e.IdAeropuertoDestino)
                .HasColumnName("id_aeropuerto_destino")
                .IsRequired();
            // Sin HasForeignKey — es referencia lógica al MS Aeropuertos

            builder.Property(e => e.NumeroVuelo)
                .HasColumnName("numero_vuelo")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.FechaHoraSalida)
                .HasColumnName("fecha_hora_salida")
                .HasColumnType("timestamp")
                .IsRequired();

            builder.Property(e => e.FechaHoraLlegada)
                .HasColumnName("fecha_hora_llegada")
                .HasColumnType("timestamp")
                .IsRequired();

            builder.Property(e => e.DuracionMin)
                .HasColumnName("duracion_min")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.PrecioBase)
                .HasColumnName("precio_base")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.CapacidadTotal)
                .HasColumnName("capacidad_total")
                .IsRequired();

            builder.Property(e => e.EstadoVuelo)
                .HasColumnName("estado_vuelo")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("PROGRAMADO");

            builder.Property(e => e.Estado)
                .HasColumnName("estado")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("ACTIVO");

            builder.Property(e => e.Eliminado)
                .HasColumnName("eliminado")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.FechaRegistroUtc)
                .HasColumnName("fecha_registro_utc")
                .HasColumnType("timestamp")
                .IsRequired()
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");

            builder.Property(e => e.CreadoPorUsuario)
                .HasColumnName("creado_por_usuario")
                .HasMaxLength(100)
                .IsRequired()
                .HasDefaultValue("SYSTEM");

            builder.Property(e => e.ModificadoPorUsuario)
                .HasColumnName("modificado_por_usuario")
                .HasMaxLength(100);

            builder.Property(e => e.FechaModificacionUtc)
                .HasColumnName("fecha_modificacion_utc")
                .HasColumnType("timestamp");

            builder.Property(e => e.ModificacionIp)
                .HasColumnName("modificacion_ip")
                .HasMaxLength(45);

            // Índices del SQL
            builder.HasIndex(e => e.IdAeropuertoOrigen)
                .HasDatabaseName("ix_vuelo_aeropuerto_origen");

            builder.HasIndex(e => e.IdAeropuertoDestino)
                .HasDatabaseName("ix_vuelo_aeropuerto_destino");

            builder.HasIndex(e => e.FechaHoraSalida)
                .HasDatabaseName("ix_vuelo_fecha_salida");
        }
    }
}