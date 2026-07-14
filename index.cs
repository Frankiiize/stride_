using System;
using System.Data.SqlClient;
using System.IO;
using DotNetEnv;
using static BCrypt.Net.BCrypt;

namespace AppVulnerable
{
    class Program
    {
        const int MaxLoginAttempts = 5;
        const int LockoutMinutes = 5;

        static string connString;
        static string usuarioLogueado;

        static void Main(string[] args)
        {
            try
            {
                Env.Load();

                connString = Environment.GetEnvironmentVariable("DB_CONN_STRING")
                    ?? throw new InvalidOperationException("Falta DB_CONN_STRING");

                MenuPrincipal();
            }
            catch (Exception ex)
            {
                Log(LogLevel.Fatal, "Error critico al iniciar la aplicacion.", ex);
                Console.WriteLine("Ocurrio un error critico al iniciar la aplicacion.");
            }
        }

        static void MenuPrincipal()
        {
            while (true)
            {
                Console.WriteLine("\n--- Sistema de Gestión ---");
                Console.WriteLine("1. Iniciar Sesión\n2. Registrar Usuario\n3. Buscar Cliente (ID)\n4. Salir");
                string opcion = Console.ReadLine();

                if (opcion == null)
                {
                    Log(LogLevel.Info, "Entrada de menu finalizada sin opcion.");
                    return;
                }

                switch (opcion)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        UserSignUp();
                        break;
                    case "3":
                        BuscarCliente();
                        break;
                    case "4":
                        return;
                    default:
                        Log(LogLevel.Warn, $"Opcion de menu invalida: {opcion}");
                        Console.WriteLine("Opcion invalida.");
                        break;
                }
            }
        }

        static void Login()
        {
            Console.Write("Usuario: ");
            string user = Console.ReadLine();

            Console.Write("Password: ");
            string pass = Console.ReadLine();

            try
            {
                DateTime now = DateTime.UtcNow;

                string sql = "SELECT Password, FailedAttempts, LockoutUntil FROM Usuarios WHERE Username = @username";

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@username", user);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Log(LogLevel.Warn, $"Login fallido para usuario inexistente: {user}");
                            Console.WriteLine("Login fallido.");
                            return;
                        }

                        string storedPassword = reader["Password"].ToString();
                        int failedAttempts = Convert.ToInt32(reader["FailedAttempts"]);
                        DateTime? lockoutUntil = reader["LockoutUntil"] == DBNull.Value
                            ? null
                            : DateTime.SpecifyKind(Convert.ToDateTime(reader["LockoutUntil"]), DateTimeKind.Utc);

                        if (lockoutUntil.HasValue && lockoutUntil.Value > now)
                        {
                            Log(LogLevel.Warn, $"Login bloqueado por intentos fallidos para usuario: {user}");
                            Console.WriteLine("Login bloqueado temporalmente. Intente mas tarde.");
                            return;
                        }

                        reader.Close();

                        if (Verify(pass, storedPassword))
                        {
                            ResetLoginAttempts(conn, user);
                            usuarioLogueado = user;
                            Log(LogLevel.Info, $"Login exitoso usuario: {user}");
                            Console.WriteLine("Login exitoso!");
                        }
                        else
                        {
                            RegisterDbFailedAttempt(conn, user, failedAttempts, now);
                            Log(LogLevel.Warn, $"Login fallido usuario: {user}");
                            Console.WriteLine("Login fallido.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Error al procesar login.", ex);
                Console.WriteLine("Ocurrio un error al procesar la solicitud.");
            }
        }

        static void UserSignUp()
        {
            Console.Write("Nuevo usuario: ");
            string user = Console.ReadLine();

            Console.Write("Password: ");
            string pass = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(user) || user.Length > 50)
            {
                Console.WriteLine("Ingrese un usuario valido.");
                return;
            }

            if (string.IsNullOrWhiteSpace(pass) || pass.Length < 6)
            {
                Console.WriteLine("Ingrese una password de al menos 6 caracteres.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    string existsSql = "SELECT COUNT(1) FROM Usuarios WHERE Username = @username";
                    SqlCommand existsCmd = new SqlCommand(existsSql, conn);
                    existsCmd.Parameters.AddWithValue("@username", user);

                    int existingUsers = Convert.ToInt32(existsCmd.ExecuteScalar());
                    if (existingUsers > 0)
                    {
                        Log(LogLevel.Warn, $"Registro rechazado por usuario existente: {user}");
                        Console.WriteLine("El usuario ya existe.");
                        return;
                    }

                    string passwordHash = HashPassword(pass, workFactor: 12);
                    string insertSql = "INSERT INTO Usuarios (Username, Password) VALUES (@username, @password)";
                    SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@username", user);
                    insertCmd.Parameters.AddWithValue("@password", passwordHash);
                    insertCmd.ExecuteNonQuery();

                    Log(LogLevel.Info, $"Usuario registrado: {user}");
                    Console.WriteLine("Usuario registrado correctamente.");
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Error al registrar usuario.", ex);
                Console.WriteLine("Ocurrio un error al procesar la solicitud.");
            }
        }

        static void BuscarCliente()
        {
            if (!IsUserAuth())
            {
                Console.WriteLine("Debe iniciar sesion para buscar clientes.");
                Log(LogLevel.Warn, "Busqueda de cliente rechazada: usuario no autenticado.");
                return;
            }

            Console.Write("Ingrese ID: ");
            string id = Console.ReadLine();

            if (!int.TryParse(id, out int clienteId))
            {
                Log(LogLevel.Warn, $"Busqueda de cliente rechazada por ID invalido: {id}");
                Console.WriteLine("Ingrese un numero valido para buscar cliente.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "SELECT Nombre FROM Clientes WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", clienteId);
                    object resultado = cmd.ExecuteScalar();
                    Log(LogLevel.Info, $"Busqueda de cliente ejecutada para Id: {clienteId}");
                    Console.WriteLine("Resultado: " + resultado);
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Error al buscar cliente.", ex);
                Console.WriteLine("Ocurrio un error al procesar la solicitud.");
            }
        }

        static bool IsUserAuth()
        {
            return !string.IsNullOrWhiteSpace(usuarioLogueado);
        }

        enum LogLevel
        {
            Fatal,
            Error,
            Warn,
            Info
        }

        static void Log(LogLevel level, string message, Exception ex = null)
        {
            string detail = ex == null ? message : $"{message}{Environment.NewLine}{ex}";
            string entry = $"{DateTime.UtcNow:O} [{level}] {detail}{Environment.NewLine}";
            File.AppendAllText("app.log", entry);
        }

        static void RegisterDbFailedAttempt(SqlConnection conn, string user, int currentFailedAttempts, DateTime now)
        {
            int nextFailedAttempts = currentFailedAttempts + 1;
            DateTime? lockoutUntil = nextFailedAttempts >= MaxLoginAttempts
                ? now.AddMinutes(LockoutMinutes)
                : null;

            string sql = @"
                UPDATE Usuarios
                SET FailedAttempts = @failedAttempts,
                    LockoutUntil = @lockoutUntil
                WHERE Username = @username";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@failedAttempts", nextFailedAttempts);
            cmd.Parameters.AddWithValue("@lockoutUntil", lockoutUntil.HasValue ? lockoutUntil.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@username", user);
            cmd.ExecuteNonQuery();

            if (lockoutUntil.HasValue)
            {
                Log(LogLevel.Warn, $"Usuario bloqueado por {LockoutMinutes} minutos: {user}");
            }
        }

        static void ResetLoginAttempts(SqlConnection conn, string user)
        {
            string sql = @"
                UPDATE Usuarios
                SET FailedAttempts = 0,
                    LockoutUntil = NULL
                WHERE Username = @username";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", user);
            cmd.ExecuteNonQuery();
        }

    }
}
