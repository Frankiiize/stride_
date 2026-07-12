USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = 'admin')
BEGIN
    CREATE LOGIN admin WITH PASSWORD = 'Password123!';
END
GO

USE TiendaDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'admin')
BEGIN
    CREATE USER admin FOR LOGIN admin;
END
GO

GRANT SELECT ON dbo.Usuarios TO admin;
GRANT SELECT ON dbo.Clientes TO admin;
GRANT UPDATE ON dbo.Usuarios(FailedAttempts, LockoutUntil) TO admin;
GO
