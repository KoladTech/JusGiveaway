using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JusGiveaway
{
    //public class MySQLHelper
    //{
    //    //   P62{mA7G^BG.bm/|
    //    public MySqlConnection GetConnection()
    //    {
    //        string connectionString = ConfigurationManager.ConnectionStrings["MyDBConnection"].ConnectionString; // For .NET Framework projects
    //                                                                                                             // string connectionString = Configuration["ConnectionStrings:MyDBConnection"]; // For .NET Core projects

    //        MySqlConnection connection = new MySqlConnection(connectionString);
    //        return connection;
    //    }
    //}

    public class MySQLHelper
    {
        private readonly string connectionString;

        //public MySQLHelper(string server, string database, string username, string password)
        //{
        //    server = "";
        //    username = "root";
        //    password = "P62{mA7G^BG.bm/|";
        //    database = "headsortails_Cloud_DB";


        //    // Construct the connection string
        //    connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
        //    connectionString = ultimate - walker - 413500:africa - south1:headsortails - mysql - cloud - db
        //}

        //public MySqlConnection GetConnection()
        //{
        //    try
        //    {
        //        // Create a new MySqlConnection object with the connection string
        //        MySqlConnection connection = new MySqlConnection(connectionString);
        //        return connection;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any exceptions that occur during connection
        //        Console.WriteLine($"Error connecting to MySQL database: {ex.Message}");
        //        return null;
        //    }
        //}

        public static void ConnectAndQuery()
        {
            string connectionString = "Server=34.35.28.117;Port=3306;Database=headsortails_Cloud_DB;Uid=root;Pwd=P62{mA7G^BG.bm/|;";

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Connection opened successfully!");

                    // Execute a sample query
                    string query = "SELECT * FROM YourTable;";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Process rows returned by the query
                                // Example: Console.WriteLine(reader.GetString(0));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
