using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microservicio.Vuelos.DataAccess.Entities;

namespace Microservicio.Vuelos.DataAccess.Configurations
{
    public class AsientoConfiguration : IEntityTypeConfiguration<AsientoEntity>
    {
        public void Configure(EntityTypeBuilder<AsientoEntity> builder)
        {
            builder.ToTable("asiento", "vuelos");

            builder.HasKey(e => e.IdAsiento)
                .HasName("pk_asiento");

            builder.Property(e => e.IdAsiento)
                .HasColumnName("id_asiento")
                .UseIdentityAlwaysColumn();

            builder.Property(e => e.IdVuelo)
                .HasColumnName("id_vuelo")
                .IsRequired();

            builder.Property(e => e.NumeroAsiento)
                .HasColumnName("numero_asiento")
                .HasMaxLength(5)
                .IsRequired();

            builder.Property(e => e.Clase)
                .HasColumnName("clase")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("ECONOMICA");

            builder.Property(e => e.Disponible)
                .HasColumnName("disponible")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.PrecioExtra)
                .HasColumnName("precio_extra")
                .HasColumnType("decimal(8,2)")
                .IsRequired()
                .HasDefaultValue(0m);

            builder.Property(e => e.Posicion)
                .HasColumnName("posicion")
                .HasMaxLength(20);

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

            builder.HasIndex(e => new { e.IdVuelo, e.NumeroAsiento })
                .IsUnique()
                .HasDatabaseName("uq_asiento_vuelo_num");

            builder.HasIndex(e => e.IdVuelo)
                .HasDatabaseName("ix_asiento_vuelo");

            builder.HasOne(e => e.Vuelo)
                .WithMany(v => v.Asientos)
                .HasForeignKey(e => e.IdVuelo)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_asiento_vuelo");
        }
    }
}