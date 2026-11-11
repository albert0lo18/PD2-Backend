-- Ejecutar una sola vez en la misma base PostgreSQL usada por GeoExpande.Api.
CREATE TABLE IF NOT EXISTS public.tokens_revocados (
    jti_hash character varying(64) NOT NULL,
    id_usuario integer NOT NULL,
    fecha_revocacion timestamp with time zone NOT NULL DEFAULT now(),
    expira_en timestamp with time zone NOT NULL,
    CONSTRAINT tokens_revocados_pkey PRIMARY KEY (jti_hash),
    CONSTRAINT fk_tokens_revocados_usuarios
        FOREIGN KEY (id_usuario) REFERENCES public.usuarios(id_usuario) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_tokens_revocados_expira_en
    ON public.tokens_revocados (expira_en);
