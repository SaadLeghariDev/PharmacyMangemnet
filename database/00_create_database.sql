/*
  Pharmacy Management System — Phase 1
  Create database (idempotent)
*/
IF DB_ID(N'PharmacyManagement') IS NULL
BEGIN
    CREATE DATABASE PharmacyManagement;
END
GO
