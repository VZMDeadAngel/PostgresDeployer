CREATE TABLE employees
(
	id INT NOT NULL DEFAULT nextval('employees_seq'::regclass)
		CONSTRAINT employees_pk
			PRIMARY KEY,
	external_key CHARACTER(50) NOT NULL
		CONSTRAINT employees_snilshash_key
			UNIQUE,
	firstname CHARACTER(50) NOT NULL,
	lastname CHARACTER(50) NOT NULL,
    gender_id INT NULL
        CONSTRAINT employees_genders_gender_id_fk
            REFERENCES genders
);

COMMENT ON TABLE employees IS 'Employees';
COMMENT ON COLUMN employees.external_key IS 'External key';
COMMENT ON COLUMN employees.firstname IS 'First name';
COMMENT ON COLUMN employees.lastname IS 'Last name';

ALTER TABLE employees OWNER TO postgres;

CREATE INDEX employees_index1
	ON employees (lastname DESC);