#!/usr/bin/env bash
set -euo pipefail

CONTAINER_NAME="${PMS_SQL_CONTAINER:-pms-sql}"
SA_PASSWORD="${MSSQL_SA_PASSWORD:-Your_strong_Password123}"
HOST_PORT="${PMS_SQL_PORT:-14333}"
IMAGE="${PMS_SQL_IMAGE:-mcr.microsoft.com/mssql/server:2022-latest}"

if docker ps -a --format '{{.Names}}' | grep -qx "$CONTAINER_NAME"; then
  if docker ps --format '{{.Names}}' | grep -qx "$CONTAINER_NAME"; then
    echo "Container $CONTAINER_NAME already running."
  else
    echo "Starting existing container $CONTAINER_NAME..."
    docker start "$CONTAINER_NAME"
  fi
else
  echo "Creating SQL Server 2022 container $CONTAINER_NAME on port $HOST_PORT..."
  docker run -d --name "$CONTAINER_NAME" \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=$SA_PASSWORD" \
    -e "MSSQL_PID=Developer" \
    -p "${HOST_PORT}:1433" \
    "$IMAGE"
fi

export PATH="$PATH:/opt/mssql-tools18/bin"
echo "Waiting for SQL Server to accept connections..."
for i in $(seq 1 60); do
  if sqlcmd -S "localhost,${HOST_PORT}" -U sa -P "$SA_PASSWORD" -C -I -Q "SELECT 1" &>/dev/null; then
    echo "SQL Server is ready."
    exit 0
  fi
  sleep 2
done
echo "ERROR: SQL Server did not become ready in time." >&2
exit 1
