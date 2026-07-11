using System;
using System.Data.SqlClient;

namespace AppVulnerable
{
    class Program
    {
        static string connString = "Server=localhost;Database=TiendaDB;User Id=admin;Password=Password123!;";

        static void Main(string[] args)
        {
            MenuPrincipal();
        }

        static void MenuPrincipal()
        {
            while (true)
            {
                Console.WriteLine("\n--- Sistema de Gestión ---");
                Console.WriteLine("1. Iniciar Sesión\n2. Buscar Cliente (ID)\n3. Salir");
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        BuscarCliente();
                        break;
                    case "3":
                        return;
                }
            }
        }

        static void Login()
        {
            Console.Write("Usuario: ");
            string user = Console.ReadLine();

            Console.Write("Password: ");
            string pass = Console.ReadLine();

            string sql = "SELECT * FROM Usuarios WHERE Username = '" + user + "' AND Password = '" + pass + "'";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);

                if (cmd.ExecuteScalar() != null)
                {
                    Console.WriteLine("Login exitoso!");
                }
                else
                {
                    Console.WriteLine("Login fallido.");
                }
            }
        }

        static void BuscarCliente()
        {
            Console.Write("Ingrese ID: ");
            string id = Console.ReadLine();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "SELECT Nombre FROM Clientes WHERE Id = " + id;
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    Console.WriteLine("Resultado: " + cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
              //se pueden ver las exepciones al usuario
                Console.WriteLine("Error: " + ex.ToString());
            }
        }
    }
}
