USE master;
GO

DECLARE @appLogin sysname = N'$(APP_DB_USER)';
DECLARE @appPassword nvarchar(128) = N'$(APP_DB_PASSWORD)';

IF @appLogin = N'' OR @appPassword = N''
BEGIN
    THROW 50000, 'Invalid APP_DB_USER & APP_DB_PASSWORD.', 1;
END;

IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = @appLogin)
BEGIN
    DECLARE @createLoginSql nvarchar(max) =
        N'CREATE LOGIN ' + QUOTENAME(@appLogin) +
        N' WITH PASSWORD = ' + QUOTENAME(@appPassword, '''') + N';';

    EXEC sp_executesql @createLoginSql;
END
ELSE
BEGIN
    DECLARE @alterLoginSql nvarchar(max) =
        N'ALTER LOGIN ' + QUOTENAME(@appLogin) +
        N' WITH PASSWORD = ' + QUOTENAME(@appPassword, '''') + N';';

    EXEC sp_executesql @alterLoginSql;
END
GO

USE TiendaDB;
GO

DECLARE @appLogin sysname = N'$(APP_DB_USER)';
DECLARE @createUserSql nvarchar(max);

IF @appLogin = N''
BEGIN
    THROW 50000, 'Invalid APP_DB_USER.', 1;
END;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @appLogin)
BEGIN
    SET @createUserSql =
        N'CREATE USER ' + QUOTENAME(@appLogin) +
        N' FOR LOGIN ' + QUOTENAME(@appLogin) + N';';

    EXEC sp_executesql @createUserSql;
END
GO

DECLARE @appLogin sysname = N'$(APP_DB_USER)';
DECLARE @grantSql nvarchar(max);

IF @appLogin = N''
BEGIN
    THROW 50000, 'Invalid APP_DB_USER.', 1;
END;

SET @grantSql =
    N'GRANT SELECT ON dbo.Usuarios TO ' + QUOTENAME(@appLogin) + N';
      GRANT SELECT ON dbo.Clientes TO ' + QUOTENAME(@appLogin) + N';
      GRANT INSERT ON dbo.Usuarios TO ' + QUOTENAME(@appLogin) + N';
      GRANT UPDATE ON dbo.Usuarios(FailedAttempts, LockoutUntil) TO ' + QUOTENAME(@appLogin) + N';';

EXEC sp_executesql @grantSql;
GO
