-- Ejecutar una vez en la misma base PostgreSQL que usa GeoExpande.Api.
-- El PDF se guarda en bytea para que se pueda volver a descargar sin depender
-- del disco local del servidor ni de un servicio de archivos externo.
CREATE TABLE IF NOT EXISTS public.reportes (
    id_reporte integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_analisis integer NOT NULL,
    id_usuario integer NOT NULL,
    incluir_mapas boolean NOT NULL DEFAULT false,
    incluir_proveedores boolean NOT NULL DEFAULT false,
    fecha_generacion timestamp with time zone NOT NULL DEFAULT now(),
    nombre_archivo character varying(255) NOT NULL,
    archivo_pdf bytea NOT NULL,
    CONSTRAINT fk_reportes_analisis
        FOREIGN KEY (id_analisis) REFERENCES public.analisis(id_analisis) ON DELETE CASCADE,
    CONSTRAINT fk_reportes_usuarios
        FOREIGN KEY (id_usuario) REFERENCES public.usuarios(id_usuario) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_reportes_usuario_fecha
    ON public.reportes (id_usuario, fecha_generacion DESC);

ALTER TABLE public.reportes ENABLE ROW LEVEL SECURITY;
