# Requisitos

Antes de comenzar, asegúrate de tener instalado:

- .NET 8 SDK
- Docker y Docker Compose

---

# Configuración inicial

1. Crear el archivo de variables de entorno:

```bash
cp .env.example .env
```

2. Levantar el contenedor de SQL Server:

```bash
docker compose up -d
```

3. Crear la base de datos y cargar la configuración inicial:

```bash
docker compose exec sqlserver bash -lc 'cd /scripts && SQLCMD=/opt/mssql-tools18/bin/sqlcmd; [ -x "$SQLCMD" ] || SQLCMD=/opt/mssql-tools/bin/sqlcmd; "$SQLCMD" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i setup.sql -v APP_DB_USER="$APP_DB_USER" APP_DB_PASSWORD="$APP_DB_PASSWORD"'
```

---

# Ejecutar la aplicación

Desde la raíz del proyecto:

```bash
dotnet run
```

---

# Administración del entorno Docker

## Detener los contenedores

```bash
docker compose down
```

## Volver a iniciar el entorno

```bash
docker compose up -d
```

## Verificar el estado de los contenedores

```bash
docker compose ps
```

## Ver los logs de SQL Server

```bash
docker compose logs -f sqlserver
```