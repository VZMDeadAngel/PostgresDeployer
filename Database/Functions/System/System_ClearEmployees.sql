CREATE OR REPLACE FUNCTION "System_ClearEmployees"() RETURNS VOID
    LANGUAGE PLPGSQL
AS
$$
BEGIN
    --//////////////////////////////////////

    DELETE FROM public.employees;

    --//////////////////////////////////////
END;
$$;