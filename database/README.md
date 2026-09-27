# Pharmacy Management System — Phase 1 (Database)

SQL Server 2022 schema for the Pharmacy Management System (~108 tables in `dbo`). Phase 1 ships **database only**: DDL, constraints, indexes, seed data, and automated T-SQL tests.

## Prerequisites

- Docker
- `sqlcmd` (mssql-tools18)
- Ports: host `14333` → container `1433`

Default SA password (local/dev only): `Your_strong_Password123`

## Quick start

```bash
cd database/scripts
./docker-up.sh          # start SQL Server 2022 container
./apply_all.sh          # create DB, apply DDL 00–18, seed 19
./run_tests.sh          # wipe transactional data, re-seed, run 35 tests
```

## Layout

```text
database/
  00_create_database.sql … 19_seed_data.sql
  tests/                  # 35 mandatory scenarios
  scripts/                # docker-up, apply_all, run_tests
```

## Notes

- Single schema: `dbo`
- Circular FKs deferred in `18_constraints_indexes.sql`
- `IdempotencyKeys.[Key]` is quoted (reserved word)
- ROWVERSION on Tenants, Branches, Users, Products, InventoryBatches, InventoryBatchLocations, Sales
- Seed includes units, payment methods, permissions, account types, device types, reason codes, and a demo org

See the Phase 1 implementation report in the project docs store for counts, test results, and schema findings.
