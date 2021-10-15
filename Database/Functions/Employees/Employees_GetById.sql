CREATE OR REPLACE FUNCTION "Employees_GetById"(userid INTEGER)
    RETURNS TABLE
            (
                id                 INTEGER,
                external_key       CHARACTER,
                firstname          CHARACTER,
                lastname           CHARACTER,
                gender_id          INT
            )
    LANGUAGE PLPGSQL
AS
$$
BEGIN

    --//////////////////////////////////////

    RETURN QUERY
        WITH x AS
                 (
                     SELECT e.id,
                            e.external_key,
                            e.firstname,
                            e.lastname,
                            e.gender_id
                     FROM employees e
                     WHERE e.id = "Employees_GetById".userid
                 )
        SELECT x.id,
               x.external_key,
               x.firstname,
               x.lastname,
               x.gender_id
        FROM x;

    --//////////////////////////////////////
END;
$$;