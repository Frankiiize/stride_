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
        Password VARCHAR(255),
        FailedAttempts INT NOT NULL DEFAULT 0,
        LockoutUntil DATETIME2 NULL
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
