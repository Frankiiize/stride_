USE TiendaDB;
GO

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Username = 'admin')
BEGIN
    INSERT INTO Usuarios (Username, Password)
    VALUES ('admin', '$2a$12$Ka2Sz4SR8hlC6/rQka3UbOiFVyeehU4IIoWOs.MhXdjhJJTZBQg8G');
END
ELSE
BEGIN
    UPDATE Usuarios
    SET Password = '$2a$12$Ka2Sz4SR8hlC6/rQka3UbOiFVyeehU4IIoWOs.MhXdjhJJTZBQg8G'
    WHERE Username = 'admin';
END

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Username = 'juan')
BEGIN
    INSERT INTO Usuarios (Username, Password)
    VALUES ('juan', '$2a$12$M8682kPVe.4LsCPdZce7ieZceQfdUDlVI6pBz6CxAk8d01yNAQe6G');
END
ELSE
BEGIN
    UPDATE Usuarios
    SET Password = '$2a$12$M8682kPVe.4LsCPdZce7ieZceQfdUDlVI6pBz6CxAk8d01yNAQe6G'
    WHERE Username = 'juan';
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
