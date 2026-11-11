-- Ejecutar si tokens_revocados ya existe con columnas timestamp without time zone.
-- Se interpretan los valores existentes como UTC al convertirlos.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'tokens_revocados'
          AND column_name = 'fecha_revocacion'
          AND data_type = 'timestamp without time zone'
    ) THEN
        ALTER TABLE public.tokens_revocados
            ALTER COLUMN fecha_revocacion TYPE timestamp with time zone
            USING fecha_revocacion AT TIME ZONE 'UTC';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'tokens_revocados'
          AND column_name = 'expira_en'
          AND data_type = 'timestamp without time zone'
    ) THEN
        ALTER TABLE public.tokens_revocados
            ALTER COLUMN expira_en TYPE timestamp with time zone
            USING expira_en AT TIME ZONE 'UTC';
    END IF;
END $$;
