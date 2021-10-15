-- Example of a non-standard sequence
CREATE SEQUENCE public.employees_seq
    INCREMENT 2
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;