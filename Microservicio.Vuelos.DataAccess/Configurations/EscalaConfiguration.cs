using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microservicio.Vuelos.DataAccess.Entities;

namespace Microservicio.Vuelos.DataAccess.Configurations
{
    /// <summary>
    /// Configuración Fluent API para vuelos.escala.
    /// CAMBIOS MICROSERVICIO:
    ///   - Se eliminó HasOne(Aeropuerto) porque AeropuertoEntity no existe
    ///     en BDD_Vuelos. IdAeropuerto es columna INT sin CONSTRAINT FK.
    ///     La validación la hace EscalaService llamando a MS Aeropuertos vía HTTP.
    ///   - FK interna con vuelos.vuelo SÍ se conserva (misma BDD).
    ///   - Se eliminó RowVersion (SQL Server) — no aplica en PostgreSQL.
    /// </summary>
    public class EscalaConfiguration : IEntityTypeConfiguration<EscalaEntity>
    {
        public void Configure(EntityTypeBuilder<EscalaEntity> builder)
        {
            builder.ToTable("escala", "vuelos");

            builder.HasKey(e => e.IdEscala)
                .HasName("pk_escala");

            builder.Property(e => e.IdEscala)
                .HasColumnName("id_escala")
                .UseIdentityAlwaysColumn();

            builder.Property(e => e.IdVuelo)
                .HasColumnName("id_vuelo")
                .IsRequired();

            builder.Property(e => e.IdAeropuerto)
                .HasColumnName("id_aeropuerto")
                .IsRequired();
            // Sin HasForeignKey — es referencia lógica al MS Aeropuertos

            builder.Property(e => e.Orden)
                .HasColumnName("orden")
                .IsRequired();

            builder.Property(e => e.FechaHoraLlegada)
                .HasColumnName("fecha_hora_llegada")
                .HasColumnType("timestamp")
                .IsRequired();

            builder.Property(e => e.FechaHoraSalida)
                .HasColumnName("fecha_hora_salida")
                .HasColumnType("timestamp")
                .IsRequired();

            builder.Property(e => e.DuracionMin)
                .HasColumnName("duracion_min")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.TipoEscala)
                .HasColumnName("tipo_escala")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("COMERCIAL");

            builder.Property(e => e.Terminal)
                .HasColumnName("terminal")
                .HasMaxLength(50);

            builder.Property(e => e.Puerta)
                .HasColumnName("puerta")
                .HasMaxLength(10);

            builder.Property(e => e.Observaciones)
                .HasColumnName("observaciones")
                .HasMaxLength(255);

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

            // Índice único compuesto del SQL
            builder.HasIndex(e => new { e.IdVuelo, e.Orden })
                .IsUnique()
                .HasDatabaseName("uq_escala_vuelo_orden");

            // Índices del SQL
            builder.HasIndex(e => e.IdVuelo)
                .HasDatabaseName("ix_escala_vuelo");

            builder.HasIndex(e => e.IdAeropuerto)
                .HasDatabaseName("ix_escala_aeropuerto");

            // FK interna — Escala → Vuelo, misma BDD, se conserva
            builder.HasOne(e => e.Vuelo)
                .WithMany(v => v.Escalas)
                .HasForeignKey(e => e.IdVuelo)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_escala_vuelo");
        }
    }
}