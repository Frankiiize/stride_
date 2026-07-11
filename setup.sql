IF DB_ID('TiendaDB') IS NULL
BEGIN
    CREATE DATABASE TiendaDB;
END
GO

USE TiendaDB;
GO

IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE Usuarios (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Username VARCHAR(50),
        Password VARCHAR(50)
    );
END
GO

IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE Clientes (
        Id INT PRIMARY KEY,
        Nombre VARCHAR(100),
        Email VARCHAR(100)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Username = 'admin')
BEGIN
    INSERT INTO Usuarios (Username, Password) VALUES ('admin', 'admin123');
END

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Username = 'juan')
BEGIN
    INSERT INTO Usuarios (Username, Password) VALUES ('juan', 'pass123');
END

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Id = 1)
BEGIN
    INSERT INTO Clientes (Id, Nombre, Email) VALUES (1, 'Juan Donoso', 'juan@ejemplo.com');
END

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Id = 2)
BEGIN
    INSERT INTO Clientes (Id, Nombre, Email) VALUES (2, 'Constanza Herrera', 'conty@ejemplo.com');
END
GO

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
GO
