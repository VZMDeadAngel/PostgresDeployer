CREATE TABLE public.genders 
(
    id SERIAL NOT NULL
		CONSTRAINT genders_pk
			PRIMARY KEY,
    NAME CHARACTER(255) NOT NULL
);

ALTER TABLE public.genders
    ADD CONSTRAINT genders_name_unique UNIQUE (name);

COMMENT ON TABLE genders IS 'Genders';
COMMENT ON COLUMN genders.name IS 'Name of gender';

ALTER TABLE genders OWNER TO postgres;