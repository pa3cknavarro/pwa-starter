
#!/bin/bash
set -e

APP_PASSWORD="$(cat /run/secrets/dbuser_password)"
POSTGRES_DB="carovet"
POSTGRES_USER="sa"

psql \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set=ON_ERROR_STOP=1 \
  --set=app_password="$APP_PASSWORD" <<'SQL'

-- Create the application user
CREATE ROLE dbuser
    WITH LOGIN
    PASSWORD :'app_password';

-- Make the application user the database owner
ALTER DATABASE carovet OWNER TO dbuser;

-- Give the application user ownership of the default schema
ALTER SCHEMA public OWNER TO dbuser;

-- Grant database privileges
GRANT ALL PRIVILEGES
    ON DATABASE carovet
    TO dbuser;

SQL

echo "Application user created successfully."
