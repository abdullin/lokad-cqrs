using System.Data.SqlClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public static class DatabaseHelper
    {
        public static void CreateDatabase(string connectionString)
        {
            var csb = new SqlConnectionStringBuilder(connectionString);
            var databaseName = csb.InitialCatalog;
            csb.InitialCatalog = "master";

            using (var conn = new SqlConnection(csb.ConnectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "CREATE DATABASE " + databaseName;
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteDatabase(string connectionString)
        {
            var csb = new SqlConnectionStringBuilder(connectionString);
            var databaseName = csb.InitialCatalog;
            csb.InitialCatalog = "master";

            using (var conn = new SqlConnection(csb.ConnectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    // Disconnect other users from database
                    command.CommandText = "ALTER DATABASE " + databaseName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    command.ExecuteNonQuery();

                    command.CommandText = "DROP DATABASE " + databaseName;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
