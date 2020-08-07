// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Data.Sqlite;

namespace Microsoft.Zumo.Server.Test.E2EServer.Database
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
