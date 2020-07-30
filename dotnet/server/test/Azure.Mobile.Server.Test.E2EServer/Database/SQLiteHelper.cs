using Microsoft.Data.Sqlite;

namespace Azure.Mobile.Server.Test.E2EServer.Database
{
    internal static class SQLiteHelper
    {
        static SqliteConnection _connection;

        public static SqliteConnection GetConnection(string connectionString)
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(connectionString);
                _connection.Open();
            }
            return _connection;
        }
    }
}
