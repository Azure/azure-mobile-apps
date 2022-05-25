// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using SQLitePCL;

namespace SQLiteStore.Tests.Helpers
{
    internal static class TestUtilities
    {
        public static void ResetDatabase(string dbName)
        {
            DropTestTable(dbName, MobileServiceLocalSystemTables.OperationQueue);
            DropTestTable(dbName, MobileServiceLocalSystemTables.SyncErrors);
            DropTestTable(dbName, MobileServiceLocalSystemTables.Config);
        }

        public static void DropTestTable(string dbName, string tableName)
        {
            ExecuteNonQuery(dbName, "DROP TABLE IF EXISTS " + tableName);
        }

        public static long CountRows(string dbName, string tableName)
        {
            long count = 0;
            string sql = "SELECT COUNT(1) from " + tableName;
            sqlite3 connection = SQLitePCLRawHelpers.GetSqliteConnection(dbName);

            sqlite3_stmt statement = SQLitePCLRawHelpers.GetSqliteStatement(sql, connection);
            using (connection)
            {
                using (statement)
                {
                    int rc = raw.sqlite3_step(statement);
                    SQLitePCLRawHelpers.VerifySQLiteResponse(rc, raw.SQLITE_ROW, connection);
                    count = (long)raw.sqlite3_column_int64(statement, 0);
                }
            }
            return count;
        }

        public static void Truncate(string dbName, string tableName)
        {
            ExecuteNonQuery(dbName, "DELETE FROM " + tableName);
        }

        public static void ExecuteNonQuery(string dbName, string sql)
        {
            sqlite3 connection = SQLitePCLRawHelpers.GetSqliteConnection(dbName);
            sqlite3_stmt statement = SQLitePCLRawHelpers.GetSqliteStatement(sql, connection);
            using (connection)
            {
                using (statement)
                {
                    int rc = raw.sqlite3_step(statement);
                    SQLitePCLRawHelpers.VerifySQLiteResponse(rc, raw.SQLITE_DONE, connection);
                }
            }
        }
    }
}