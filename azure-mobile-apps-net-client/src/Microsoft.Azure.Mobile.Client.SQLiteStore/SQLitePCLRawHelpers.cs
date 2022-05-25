// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using SQLitePCL;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal class SQLitePCLRawHelpers
    {
        internal static bool sqliteIsInitialized = false;

        internal enum SQLiteType
        {
            INTEGER = 1, // 64-bit signed integer
            FLOAT = 2, // 64-bit IEEE floating point number
            TEXT = 3, // string
            BLOB = 4, // BLOB
            NULL = 5, // NULL
        }

        internal static sqlite3 GetSqliteConnection(string filename)
        {
            if (!sqliteIsInitialized)
            {
                Batteries_V2.Init();
                sqliteIsInitialized = true;
            }

            int rc = raw.sqlite3_open(filename, out sqlite3 connection);
            VerifySQLiteResponse(rc, raw.SQLITE_OK, connection);
            return connection;
        }

        internal static sqlite3_stmt GetSqliteStatement(string sql, sqlite3 db)
        {
            int rc = raw.sqlite3_prepare_v2(db, sql, out sqlite3_stmt statement);
            VerifySQLiteResponse(rc, raw.SQLITE_OK, db);
            return statement;
        }

        internal static void VerifySQLiteResponse(int result, int expectedResult, sqlite3 db)
        {
            if (result != expectedResult)
            {
                string sqliteErrorMessage = (db == null)
                    ? raw.sqlite3_errstr(result).utf8_to_string()
                    : raw.sqlite3_errmsg(db).utf8_to_string();
                throw new SQLiteException(string.Format("Error executing SQLite command: '{0}'.", sqliteErrorMessage));
            }
        }

        internal static void Bind(sqlite3 db, sqlite3_stmt stm, int index, object value)
        {
            int rc;
            if (value == null)
            {
                rc = raw.sqlite3_bind_null(stm, index);
            }
            else
            {
                if (IsSupportedInteger(value))
                {
                    rc = raw.sqlite3_bind_int64(stm, index, GetInteger(value));
                }
                else if (IsSupportedFloat(value))
                {
                    rc = raw.sqlite3_bind_double(stm, index, GetFloat(value));
                }
                else if (IsSupportedText(value))
                {
                    rc = raw.sqlite3_bind_text(stm, index, value.ToString());
                }
                else if (value is byte[] v)
                {
                    rc = raw.sqlite3_bind_blob(stm, index, v);
                }
                else
                {
                    throw new SQLiteException("Unable to bind parameter with unsupported type: " + value.GetType().FullName);
                }
            }

            VerifySQLiteResponse(rc, raw.SQLITE_OK, db);
        }

        private static bool IsSupportedInteger(object value)
        {
            return value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong;
        }

        private static bool IsSupportedFloat(object value)
        {
            return value is decimal || value is float || value is double;
        }

        private static bool IsSupportedText(object value)
        {
            return value is char || value is string;
        }

        private static long GetInteger(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value is ulong asUlong && asUlong > long.MaxValue)
            {
                throw new SQLiteException("Unable to cast provided ulong value. Overflow ocurred: " + value.ToString());
            }

            return Convert.ToInt64(value);
        }

        private static double GetFloat(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return Convert.ToDouble(value);
        }

        internal static object GetValue(sqlite3_stmt stm, int index)
        {
            object result = null;
            var type = (SQLiteType)raw.sqlite3_column_type(stm, index);
            switch (type)
            {
                case SQLiteType.INTEGER:
                    result = raw.sqlite3_column_int64(stm, index);
                    break;
                case SQLiteType.FLOAT:
                    result = raw.sqlite3_column_double(stm, index);
                    break;
                case SQLiteType.TEXT:
                    result = raw.sqlite3_column_text(stm, index).utf8_to_string();
                    break;
                case SQLiteType.BLOB:
                    result = raw.sqlite3_column_blob(stm, index).ToArray();
                    break;
                case SQLiteType.NULL:
                    break;
            }
            return result;
        }
    }
}
