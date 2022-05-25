// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    /// <summary>
    /// Represents an error in the SQLite sub-system
    /// </summary>
    public class SQLiteException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="SQLiteException"/>
        /// </summary>
        /// <param name="message">The message</param>
        public SQLiteException(string message)
            : base(message)
        {
        }
    }
}
