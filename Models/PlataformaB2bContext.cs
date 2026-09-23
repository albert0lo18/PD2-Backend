using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PD2_Backend.Models;

public partial class PlataformaB2bContext : DbContext
{
    public PlataformaB2bContext()
    {
    }

    public PlataformaB2bContext(DbContextOptions<PlataformaB2bContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Analisi> Analises { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<EscenariosExpansion> EscenariosExpansions { get; set; }

    public virtual DbSet<EstablecimientosDenue> EstablecimientosDenues { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Giro> Giros { get; set; }

    public virtual DbSet<Localidade> Localidades { get; set; }

    public virtual DbSet<Municipio> Municipios { get; set; }

    public virtual DbSet<PreferenciasExpansion> PreferenciasExpansions { get; set; }

    public virtual DbSet<ProveedorGiro> ProveedorGiros { get; set; }

    public virtual DbSet<ProveedorSector> ProveedorSectors { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<ProveedoresRecomendado> ProveedoresRecomendados { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sectore> Sectores { get; set; }

    public virtual DbSet<UbicacionesRecomendada> UbicacionesRecomendadas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:ConexionSQL");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Analisi>(entity =>
        {
            entity.HasKey(e => e.IdAnalisis).HasName("PK__Analisis__EFD67EA61ADCC0FA");

            entity.ToTable("Analisis");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("En proceso");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Analisis)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Analisis_Empresa");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Analisis)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Analisis_Usuario");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("PK__Empresas__5EF4033E209FC76E");

            entity.HasIndex(e => e.Rfc, "UQ_Empresas_RFC").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RFC");
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.IdGiroNavigation).WithMany(p => p.Empresas)
                .HasForeignKey(d => d.IdGiro)
                .HasConstraintName("FK_Empresas_Giro");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.Empresas)
                .HasForeignKey(d => d.IdSector)
                .HasConstraintName("FK_Empresas_Sector");
        });

        modelBuilder.Entity<EscenariosExpansion>(entity =>
        {
            entity.HasKey(e => e.IdEscenario).HasName("PK__Escenari__0F4AAB959DA1D55B");

            entity.ToTable("EscenariosExpansion");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Presupuesto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RadioBusquedaKm).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdAnalisisNavigation).WithMany(p => p.EscenariosExpansions)
                .HasForeignKey(d => d.IdAnalisis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Escenarios_Analisis");
        });

        modelBuilder.Entity<EstablecimientosDenue>(entity =>
        {
            entity.HasKey(e => e.IdEstablecimiento).HasName("PK__Establec__5C5599D4E5AD11F1");

            entity.ToTable("EstablecimientosDENUE");

            entity.HasIndex(e => e.CodigoActividad, "IX_DENUE_Actividad");

            entity.HasIndex(e => e.IdEstado, "IX_DENUE_Estado");

            entity.HasIndex(e => e.IdMunicipio, "IX_DENUE_Municipio");

            entity.Property(e => e.ActividadEconomica)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Calle)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CodigoActividad)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Colonia)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CorredorIndustrial)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Latitud).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Nombre)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NumeroExterior)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NumeroInterior)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Sector)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.TipoEstablecimiento)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TipoVialidad)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DENUE_Estado");

            entity.HasOne(d => d.IdLocalidadNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdLocalidad)
                .HasConstraintName("FK_DENUE_Localidad");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DENUE_Municipio");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__Estados__FBB0EDC12D9B0FED");

            entity.HasIndex(e => e.Nombre, "UQ__Estados__75E3EFCFDB6B9815").IsUnique();

            entity.HasIndex(e => e.Clave, "UQ__Estados__E8181E118DA9F164").IsUnique();

            entity.Property(e => e.Clave)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Giro>(entity =>
        {
            entity.HasKey(e => e.IdGiro).HasName("PK__Giros__98AAAF2CCE35859B");

            entity.HasIndex(e => new { e.IdSector, e.Nombre }, "UQ_Giro_Sector").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.Giros)
                .HasForeignKey(d => d.IdSector)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Giros_Sectores");
        });

        modelBuilder.Entity<Localidade>(entity =>
        {
            entity.HasKey(e => e.IdLocalidad).HasName("PK__Localida__27432612ADF884CC");

            entity.Property(e => e.Clave)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Localidades)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Localidades_Municipios");
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.IdMunicipio).HasName("PK__Municipi__61005978C5A47175");

            entity.HasIndex(e => new { e.IdEstado, e.Clave }, "UQ_Municipio_Estado").IsUnique();

            entity.Property(e => e.Clave)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Municipios)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Municipios_Estados");
        });

        modelBuilder.Entity<PreferenciasExpansion>(entity =>
        {
            entity.HasKey(e => e.IdPreferencia).HasName("PK__Preferen__E014658072A7D711");

            entity.ToTable("PreferenciasExpansion");

            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PresupuestoMaximo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PresupuestoMinimo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrioridadAccesibilidad).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PrioridadCompetencia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PrioridadPoblacion).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PrioridadProveedores).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.RadioAnalisisKm).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.PreferenciasExpansions)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Preferencias_Empresas");
        });

        modelBuilder.Entity<ProveedorGiro>(entity =>
        {
            entity.HasKey(e => new { e.IdProveedor, e.IdGiro });

            entity.ToTable("ProveedorGiro");

            entity.Property(e => e.ProductoServicio)
                .HasMaxLength(300)
                .IsUnicode(false);

            entity.HasOne(d => d.IdGiroNavigation).WithMany(p => p.ProveedorGiros)
                .HasForeignKey(d => d.IdGiro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProveedorGiro_Giro");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.ProveedorGiros)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProveedorGiro_Proveedor");
        });

        modelBuilder.Entity<ProveedorSector>(entity =>
        {
            entity.HasKey(e => new { e.IdProveedor, e.IdSector });

            entity.ToTable("ProveedorSector");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.ProveedorSectors)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProveedorSector_Proveedor");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.ProveedorSectors)
                .HasForeignKey(d => d.IdSector)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProveedorSector_Sector");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__Proveedo__E8B631AF482496C3");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Calle)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Colonia)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NumeroExterior)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RFC");
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Proveedores)
                .HasForeignKey(d => d.IdEstado)
                .HasConstraintName("FK_Proveedores_Estado");

            entity.HasOne(d => d.IdLocalidadNavigation).WithMany(p => p.Proveedores)
                .HasForeignKey(d => d.IdLocalidad)
                .HasConstraintName("FK_Proveedores_Localidad");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Proveedores)
                .HasForeignKey(d => d.IdMunicipio)
                .HasConstraintName("FK_Proveedores_Municipio");
        });

        modelBuilder.Entity<ProveedoresRecomendado>(entity =>
        {
            entity.HasKey(e => e.IdRecomendacion).HasName("PK__Proveedo__190283E01BAE3B39");

            entity.Property(e => e.DistanciaKm).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FechaCalculo).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Motivo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Puntuacion).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.ProveedoresRecomendados)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProvRec_Proveedor");

            entity.HasOne(d => d.IdUbicacionNavigation).WithMany(p => p.ProveedoresRecomendados)
                .HasForeignKey(d => d.IdUbicacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProvRec_Ubicacion");
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.IdReporte).HasName("PK__Reportes__F95611362D59766B");

            entity.Property(e => e.FechaGeneracion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdAnalisisNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdAnalisis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Analisis");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Usuario");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__2A49584CE9AF4759");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCF360D6D4F").IsUnique();

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Sectore>(entity =>
        {
            entity.HasKey(e => e.IdSector).HasName("PK__Sectores__D0011C18B828EE93");

            entity.HasIndex(e => e.Nombre, "UQ__Sectores__75E3EFCF804C4133").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UbicacionesRecomendada>(entity =>
        {
            entity.HasKey(e => e.IdUbicacion).HasName("PK__Ubicacio__778CAB1D95F96F0B");

            entity.Property(e => e.CuotaPresenciaEstimada).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.FechaCalculo).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NombreUbicacion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Puntuacion).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.PuntuacionAccesibilidad).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.PuntuacionCompetencia).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.PuntuacionPoblacion).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.PuntuacionProveedores).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.IdEscenarioNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdEscenario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ubicaciones_Escenario");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ubicaciones_Estado");

            entity.HasOne(d => d.IdLocalidadNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdLocalidad)
                .HasConstraintName("FK_Ubicaciones_Localidad");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ubicaciones_Municipio");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF977897B148");

            entity.HasIndex(e => e.Correo, "UQ_Usuarios_Correo").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Usuarios_Empresas");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
