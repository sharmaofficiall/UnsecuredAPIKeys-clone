-- create_postgres_db.sql
-- Creates a dedicated user and database for UnsecuredAPIKeys.
-- Default credentials in this script: user `postgres`, password `sunny123`.
-- Run locally with: psql -h localhost -U postgres -f create_postgres_db.sql
-- Or when using the Docker container: docker exec -i unsecured-api-keys-db psql -U postgres -f /path/inside/container/create_postgres_db.sql

-- Create role if it doesn't exist (safe to run inside DO)
DO
$do$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'postgres') THEN
        CREATE ROLE postgres LOGIN PASSWORD 'sunny123';
    END IF;
END
$do$;

-- Create database if it doesn't exist. Use psql's \gexec to execute the generated CREATE DATABASE
-- statement outside the current transaction/function context.
SELECT 'CREATE DATABASE "UnsecuredAPIKeys" OWNER postgres'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'UnsecuredAPIKeys')
\gexec

-- Ensure privileges are granted (will execute only if DB exists now)
SELECT 'GRANT ALL PRIVILEGES ON DATABASE "UnsecuredAPIKeys" TO postgres'
WHERE EXISTS (SELECT FROM pg_database WHERE datname = 'UnsecuredAPIKeys')
\gexec

-- Notes:
-- 1) Replace the password with a secure one before production use or manage via environment/secret store.
-- 2) If running with Docker, you can copy this file into the container and run psql there, or run it from host using `psql` if Postgres is reachable.
