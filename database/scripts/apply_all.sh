#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DB_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
HOST_PORT="${PMS_SQL_PORT:-14333}"
SA_PASSWORD="${MSSQL_SA_PASSWORD:-Your_strong_Password123}"
SERVER="localhost,${HOST_PORT}"
export PATH="$PATH:/opt/mssql-tools18/bin"

SQLCMD=(sqlcmd -S "$SERVER" -U sa -P "$SA_PASSWORD" -C -I -b)

run_file() {
  local f="$1"
  echo ">>> Applying $(basename "$f")"
  "${SQLCMD[@]}" -i "$f"
}

FILES=(
  00_create_database.sql
  01_schemas_and_types.sql
  02_organization.sql
  03_security.sql
  04_product_master.sql
  05_units_barcodes.sql
  06_pricing_tax.sql
  07_procurement.sql
  08_inventory.sql
  09_pos_sales.sql
  10_customers.sql
  11_prescription.sql
  12_controlled.sql
  13_finance.sql
  14_fiscal.sql
  15_hardware.sql
  16_alerts_offline.sql
  17_common.sql
  18_constraints_indexes.sql
  19_seed_data.sql
)

for f in "${FILES[@]}"; do
  run_file "$DB_DIR/$f"
done

echo "All DDL + seed scripts applied."
