#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DB_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
HOST_PORT="${PMS_SQL_PORT:-14333}"
SA_PASSWORD="${MSSQL_SA_PASSWORD:-Your_strong_Password123}"
SERVER="localhost,${HOST_PORT}"
export PATH="$PATH:/opt/mssql-tools18/bin"

OUT_DIR="${PMS_TEST_OUT:-/tmp/pms-test-results}"
mkdir -p "$OUT_DIR"
RESULT_FILE="$OUT_DIR/results.txt"
: > "$RESULT_FILE"

SQLCMD=(sqlcmd -S "$SERVER" -U sa -P "$SA_PASSWORD" -C -I -d PharmacyManagement)
SQLCMD_STRICT=(sqlcmd -S "$SERVER" -U sa -P "$SA_PASSWORD" -C -I -b -d PharmacyManagement)

echo "Resetting and seeding test baseline..."
"${SQLCMD_STRICT[@]}" -i "$DB_DIR/tests/00_reset_and_seed.sql"
"${SQLCMD_STRICT[@]}" -i "$DB_DIR/19_seed_data.sql"

PASS=0
FAIL=0
TOTAL=0

run_suite() {
  local file="$1"
  local label
  label="$(basename "$file")"
  echo "=== Running $label ==="
  local out
  set +e
  out="$("${SQLCMD[@]}" -i "$file" 2>&1)"
  local rc=$?
  set -e
  echo "$out" | tee "$OUT_DIR/${label}.log"
  # Parse TEST_PASS / TEST_FAIL markers
  local p f
  p=$(echo "$out" | grep -c 'TEST_PASS:' || true)
  f=$(echo "$out" | grep -c 'TEST_FAIL:' || true)
  PASS=$((PASS + p))
  FAIL=$((FAIL + f))
  TOTAL=$((TOTAL + p + f))
  if [[ $rc -ne 0 && $f -eq 0 && $p -eq 0 ]]; then
    echo "TEST_FAIL: suite $label exited rc=$rc (no test markers)" | tee -a "$RESULT_FILE"
    FAIL=$((FAIL + 1))
    TOTAL=$((TOTAL + 1))
  elif [[ $rc -ne 0 && $f -eq 0 && $p -gt 0 ]]; then
    echo "TEST_FAIL: suite $label aborted after partial passes (rc=$rc)" | tee -a "$RESULT_FILE"
    FAIL=$((FAIL + 1))
    TOTAL=$((TOTAL + 1))
  fi
  echo "$out" | grep -E 'TEST_PASS:|TEST_FAIL:' >> "$RESULT_FILE" || true
}

for f in \
  "$DB_DIR/tests/01_org_product_setup.sql" \
  "$DB_DIR/tests/02_purchase_receive.sql" \
  "$DB_DIR/tests/03_sales_fefo.sql" \
  "$DB_DIR/tests/04_returns_inventory.sql" \
  "$DB_DIR/tests/05_rx_controlled_cash.sql" \
  "$DB_DIR/tests/06_accounting_fiscal_sync.sql" \
  "$DB_DIR/tests/07_barcode_hardware_audit.sql"
do
  run_suite "$f"
done

echo ""
echo "========================================"
echo "TEST SUMMARY: Passed=$PASS Failed=$FAIL Total=$TOTAL"
echo "========================================"
echo "Passed=$PASS Failed=$FAIL Total=$TOTAL" > "$OUT_DIR/summary.txt"
cat "$RESULT_FILE"

if [[ "$FAIL" -gt 0 ]]; then
  exit 1
fi
exit 0
