using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GeoExpande.Api.Dtos;

namespace GeoExpande.Api.Models;

public partial class GeoExpandeDbContext : DbContext
{
    public GeoExpandeDbContext()
    {
    }

    public GeoExpandeDbContext(DbContextOptions<GeoExpandeDbContext> options)
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

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sectore> Sectores { get; set; }

    public virtual DbSet<UbicacionesRecomendada> UbicacionesRecomendadas { get; set; }

    public virtual DbSet<RecomendacionZonaDto> RecomendacionesZonas => Set<RecomendacionZonaDto>();

    public virtual DbSet<Usuario> Usuarios { get; set; }

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
    }
}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecomendacionZonaDto>(entity =>
        {
            entity.HasNoKey();
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");
            entity.Property(e => e.NombreZona).HasColumnName("nombre_zona");
            entity.Property(e => e.Lat).HasColumnName("lat");
            entity.Property(e => e.Lng).HasColumnName("lng");
            entity.Property(e => e.Poblacion).HasColumnName("poblacion");
            entity.Property(e => e.CompetidoresCercanos).HasColumnName("competidores_cercanos");
            entity.Property(e => e.NegociosCercanosTotal).HasColumnName("negocios_cercanos_total");
            entity.Property(e => e.PuntuacionCompetencia).HasColumnName("puntuacion_competencia");
            entity.Property(e => e.PuntuacionPoblacion).HasColumnName("puntuacion_poblacion");
            entity.Property(e => e.PuntuacionAccesibilidad).HasColumnName("puntuacion_accesibilidad");
            entity.Property(e => e.Puntuacion).HasColumnName("puntuacion");
            entity.Property(e => e.CuotaPresenciaEstimada).HasColumnName("cuota_presencia_estimada");
            entity.Property(e => e.DemandaEstimadaMensualMxn).HasColumnName("demanda_estimada_mensual_mxn");
            entity.Property(e => e.NivelRiesgo).HasColumnName("nivel_riesgo");
            entity.Property(e => e.Recomendacion).HasColumnName("recomendacion");
        });

        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone", "recovery_code" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in", "like", "ilike", "is", "match", "imatch", "isdistinct" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "postgis")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<Analisi>(entity =>
        {
            entity.HasKey(e => e.IdAnalisis).HasName("analisis_pkey");

            entity.ToTable("analisis");

            entity.Property(e => e.IdAnalisis)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_analisis");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValueSql("'En proceso'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFinalizacion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_finalizacion");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Analisis)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_analisis_empresa");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Analisis)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_analisis_usuario");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("empresas_pkey");

            entity.ToTable("empresas");

            entity.HasIndex(e => e.Rfc, "uq_empresas_rfc").IsUnique();

            entity.Property(e => e.IdEmpresa)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_empresa");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CiudadBase)
                .HasMaxLength(150)
                .HasColumnName("ciudad_base");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.DescripcionNegocio)
                .HasMaxLength(1000)
                .HasColumnName("descripcion_negocio");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdGiro).HasColumnName("id_giro");
            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(200)
                .HasColumnName("nombre_comercial");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .HasColumnName("razon_social");
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .HasColumnName("rfc");
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(250)
                .HasColumnName("sitio_web");
            entity.Property(e => e.Tamano)
                .HasMaxLength(20)
                .HasColumnName("tamano");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdGiroNavigation).WithMany(p => p.Empresas)
                .HasForeignKey(d => d.IdGiro)
                .HasConstraintName("fk_empresas_giro");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.Empresas)
                .HasForeignKey(d => d.IdSector)
                .HasConstraintName("fk_empresas_sector");
        });

        modelBuilder.Entity<EscenariosExpansion>(entity =>
        {
            entity.HasKey(e => e.IdEscenario).HasName("escenarios_expansion_pkey");

            entity.ToTable("escenarios_expansion");

            entity.Property(e => e.IdEscenario)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_escenario");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdAnalisis).HasColumnName("id_analisis");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");
            entity.Property(e => e.NumeroUnidades).HasColumnName("numero_unidades");
            entity.Property(e => e.Presupuesto)
                .HasPrecision(18, 2)
                .HasColumnName("presupuesto");
            entity.Property(e => e.RadioBusquedaKm)
                .HasPrecision(10, 2)
                .HasColumnName("radio_busqueda_km");

            entity.HasOne(d => d.IdAnalisisNavigation).WithMany(p => p.EscenariosExpansions)
                .HasForeignKey(d => d.IdAnalisis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_escenarios_analisis");
        });

        modelBuilder.Entity<EstablecimientosDenue>(entity =>
        {
            entity.HasKey(e => e.IdEstablecimiento).HasName("establecimientos_denue_pkey");

            entity.ToTable("establecimientos_denue");

            entity.HasIndex(e => e.CodigoActividad, "ix_denue_actividad");

            entity.HasIndex(e => e.IdEstado, "ix_denue_estado");

            entity.HasIndex(e => e.IdGiro, "ix_denue_id_giro");

            entity.HasIndex(e => e.IdMunicipio, "ix_denue_municipio");

            entity.HasIndex(e => e.Clee, "uq_denue_clee").IsUnique();

            entity.Property(e => e.IdEstablecimiento)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_establecimiento");
            entity.Property(e => e.ActividadEconomica)
                .HasMaxLength(300)
                .HasColumnName("actividad_economica");
            entity.Property(e => e.Calle)
                .HasMaxLength(200)
                .HasColumnName("calle");
            entity.Property(e => e.Clee)
                .HasMaxLength(20)
                .HasColumnName("clee");
            entity.Property(e => e.CodigoActividad)
                .HasMaxLength(20)
                .HasColumnName("codigo_actividad");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .HasColumnName("codigo_postal");
            entity.Property(e => e.Colonia)
                .HasMaxLength(150)
                .HasColumnName("colonia");
            entity.Property(e => e.CorredorIndustrial)
                .HasMaxLength(200)
                .HasColumnName("corredor_industrial");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdGiro).HasColumnName("id_giro");
            entity.Property(e => e.IdLocalidad).HasColumnName("id_localidad");
            entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");
            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.Latitud)
                .HasPrecision(10, 7)
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasPrecision(10, 7)
                .HasColumnName("longitud");
            entity.Property(e => e.Nombre)
                .HasMaxLength(250)
                .HasColumnName("nombre");
            entity.Property(e => e.NumeroExterior)
                .HasMaxLength(30)
                .HasColumnName("numero_exterior");
            entity.Property(e => e.NumeroInterior)
                .HasMaxLength(30)
                .HasColumnName("numero_interior");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .HasColumnName("razon_social");
            entity.Property(e => e.Sector)
                .HasMaxLength(150)
                .HasColumnName("sector");
            entity.Property(e => e.TipoEstablecimiento)
                .HasMaxLength(100)
                .HasColumnName("tipo_establecimiento");
            entity.Property(e => e.TipoVialidad)
                .HasMaxLength(100)
                .HasColumnName("tipo_vialidad");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_denue_estado");

            entity.HasOne(d => d.IdGiroNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdGiro)
                .HasConstraintName("fk_denue_giro");

            entity.HasOne(d => d.IdLocalidadNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdLocalidad)
                .HasConstraintName("fk_denue_localidad");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_denue_municipio");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.EstablecimientosDenues)
                .HasForeignKey(d => d.IdSector)
                .HasConstraintName("fk_denue_sector");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("estados_pkey");

            entity.ToTable("estados");

            entity.HasIndex(e => e.Clave, "estados_clave_key").IsUnique();

            entity.HasIndex(e => e.Nombre, "estados_nombre_key").IsUnique();

            entity.Property(e => e.IdEstado)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_estado");
            entity.Property(e => e.Clave)
                .HasMaxLength(2)
                .HasColumnName("clave");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Giro>(entity =>
        {
            entity.HasKey(e => e.IdGiro).HasName("giros_pkey");

            entity.ToTable("giros");

            entity.HasIndex(e => new { e.IdSector, e.Nombre }, "uq_giro_sector").IsUnique();

            entity.HasIndex(e => e.Codigo, "uq_giros_codigo").IsUnique();

            entity.Property(e => e.IdGiro)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_giro");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.Giros)
                .HasForeignKey(d => d.IdSector)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_giros_sectores");
        });

        modelBuilder.Entity<Localidade>(entity =>
        {
            entity.HasKey(e => e.IdLocalidad).HasName("localidades_pkey");

            entity.ToTable("localidades");

            entity.Property(e => e.IdLocalidad)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_localidad");
            entity.Property(e => e.Clave)
                .HasMaxLength(10)
                .HasColumnName("clave");
            entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Localidades)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_localidades_municipios");
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.IdMunicipio).HasName("municipios_pkey");

            entity.ToTable("municipios");

            entity.HasIndex(e => new { e.IdEstado, e.Clave }, "uq_municipio_estado").IsUnique();

            entity.Property(e => e.IdMunicipio)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_municipio");
            entity.Property(e => e.Clave)
                .HasMaxLength(3)
                .HasColumnName("clave");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Poblacion).HasColumnName("poblacion");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Municipios)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_municipios_estados");
        });

        modelBuilder.Entity<PreferenciasExpansion>(entity =>
        {
            entity.HasKey(e => e.IdPreferencia).HasName("preferencias_expansion_pkey");

            entity.ToTable("preferencias_expansion");

            entity.Property(e => e.IdPreferencia)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_preferencia");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.NumeroUnidadesDeseadas).HasColumnName("numero_unidades_deseadas");
            entity.Property(e => e.PresupuestoMaximo)
                .HasPrecision(18, 2)
                .HasColumnName("presupuesto_maximo");
            entity.Property(e => e.PresupuestoMinimo)
                .HasPrecision(18, 2)
                .HasColumnName("presupuesto_minimo");
            entity.Property(e => e.PrioridadAccesibilidad)
                .HasPrecision(5, 2)
                .HasColumnName("prioridad_accesibilidad");
            entity.Property(e => e.PrioridadCompetencia)
                .HasPrecision(5, 2)
                .HasColumnName("prioridad_competencia");
            entity.Property(e => e.PrioridadPoblacion)
                .HasPrecision(5, 2)
                .HasColumnName("prioridad_poblacion");
            entity.Property(e => e.PrioridadProveedores)
                .HasPrecision(5, 2)
                .HasColumnName("prioridad_proveedores");
            entity.Property(e => e.RadioAnalisisKm)
                .HasPrecision(10, 2)
                .HasColumnName("radio_analisis_km");
            entity.Property(e => e.RegionesInteresTexto)
                .HasMaxLength(500)
                .HasColumnName("regiones_interes_texto");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.PreferenciasExpansions)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_preferencias_empresas");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Nombre, "roles_nombre_key").IsUnique();

            entity.Property(e => e.IdRol)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_rol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Sectore>(entity =>
        {
            entity.HasKey(e => e.IdSector).HasName("sectores_pkey");

            entity.ToTable("sectores");

            entity.HasIndex(e => e.Nombre, "sectores_nombre_key").IsUnique();

            entity.HasIndex(e => e.Codigo, "uq_sectores_codigo").IsUnique();

            entity.Property(e => e.IdSector)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_sector");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<UbicacionesRecomendada>(entity =>
        {
            entity.HasKey(e => e.IdUbicacion).HasName("ubicaciones_recomendadas_pkey");

            entity.ToTable("ubicaciones_recomendadas");

            entity.Property(e => e.IdUbicacion)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_ubicacion");
            entity.Property(e => e.CompetidoresCercanos).HasColumnName("competidores_cercanos");
            entity.Property(e => e.CuotaPresenciaEstimada)
                .HasPrecision(8, 2)
                .HasColumnName("cuota_presencia_estimada");
            entity.Property(e => e.DemandaEstimadaMensualMxn)
                .HasPrecision(18, 2)
                .HasColumnName("demanda_estimada_mensual_mxn");
            entity.Property(e => e.FechaCalculo)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_calculo");
            entity.Property(e => e.IdEscenario).HasColumnName("id_escenario");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdLocalidad).HasColumnName("id_localidad");
            entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");
            entity.Property(e => e.NivelRiesgo)
                .HasMaxLength(10)
                .HasColumnName("nivel_riesgo");
            entity.Property(e => e.NombreUbicacion)
                .HasMaxLength(250)
                .HasColumnName("nombre_ubicacion");
            entity.Property(e => e.ProveedoresCercanos).HasColumnName("proveedores_cercanos");
            entity.Property(e => e.Puntuacion)
                .HasPrecision(8, 2)
                .HasColumnName("puntuacion");
            entity.Property(e => e.PuntuacionAccesibilidad)
                .HasPrecision(8, 2)
                .HasColumnName("puntuacion_accesibilidad");
            entity.Property(e => e.PuntuacionCompetencia)
                .HasPrecision(8, 2)
                .HasColumnName("puntuacion_competencia");
            entity.Property(e => e.PuntuacionPoblacion)
                .HasPrecision(8, 2)
                .HasColumnName("puntuacion_poblacion");
            entity.Property(e => e.PuntuacionProveedores)
                .HasPrecision(8, 2)
                .HasColumnName("puntuacion_proveedores");
            entity.Property(e => e.Ranking).HasColumnName("ranking");
            entity.Property(e => e.Recomendacion)
                .HasMaxLength(1000)
                .HasColumnName("recomendacion");

            entity.HasOne(d => d.IdEscenarioNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdEscenario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ubicaciones_escenario");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ubicaciones_estado");

            entity.HasOne(d => d.IdLocalidadNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdLocalidad)
                .HasConstraintName("fk_ubicaciones_localidad");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.UbicacionesRecomendada)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ubicaciones_municipio");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "uq_usuarios_correo").IsUnique();

            entity.Property(e => e.IdUsuario)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .HasColumnName("apellido");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .HasColumnName("password_hash");
            entity.Property(e => e.UltimoAcceso)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ultimo_acceso");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("fk_usuarios_empresas");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuarios_roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
