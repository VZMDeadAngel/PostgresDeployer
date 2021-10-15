-- Creating recurring tasks is not currently supported

DO
$$
    DECLARE
        jid      INTEGER;
        scid     INTEGER;
        job_name TEXT = 'ClearEmployeesTable';
        sql_text TEXT = 'select public."System_ClearEmployees"()';
    BEGIN
        -- delete old job
        DELETE FROM pgagent.pga_job WHERE jobname = job_name;

        -- Creating a new job
        INSERT INTO pgagent.pga_job(jobjclid, jobname, jobdesc, jobhostagent, jobenabled)
        VALUES (1::integer, job_name::text, ''::text, ''::text, true)
        RETURNING jobid INTO jid;

        -- Steps
        -- Inserting a step (jobid: NULL)
        INSERT INTO pgagent.pga_jobstep (jstjobid, jstname, jstenabled, jstkind,
                                         jstconnstr, jstdbname, jstonerror,
                                         jstcode, jstdesc)
        VALUES (jid, 'Step1'::text, true, 's'::character(1),
                '%%ConnectionString%%'::text, ''::name, 'f'::character(1),
                sql_text, ''::text);

        -- Schedules
        -- Inserting a schedule
        INSERT INTO pgagent.pga_schedule(jscjobid, jscname, jscdesc, jscenabled,
                                         jscstart, jscminutes, jschours, jscweekdays, jscmonthdays, jscmonths)
        VALUES (jid, 'Schedule 1'::text, ''::text, true,
                '2021-01-01 00:00:00'::timestamp without time zone,
                   -- Minutes
                ARRAY [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false]::boolean[],
                   -- Hours
                ARRAY [false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false]::boolean[],
                   -- Week days
                ARRAY [true, true, true, true, true, true, true]::boolean[],
                   -- Month days
                ARRAY [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false]::boolean[],
                   -- Months
                ARRAY [false, false, false, false, false, false, false, false, false, false, false, false]::boolean[])
        RETURNING jscid INTO scid;
    END
$$;