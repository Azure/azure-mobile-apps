// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// An object to represent the current position of pull in full query resullt
    /// </summary>
    internal class PullCursor
    {
        private int initialSkip;
        private int totalRead; // used to track how many we have read so far since the last delta
        public int Remaining { get; private set; }

        public int Position => initialSkip + totalRead;
        public bool Complete => Remaining <= 0;

        public PullCursor(MobileServiceTableQueryDescription query)
        {
            Remaining = query.Top.GetValueOrDefault(Int32.MaxValue);
            initialSkip = query.Skip.GetValueOrDefault();
        }

        /// <summary>
        /// Called when ever an item is processed from result
        /// </summary>
        /// <returns>True if cursor is still open, False when it is completed.</returns>
        public bool OnNext()
        {
            if (Complete)
            {
                return false;
            }

            Remaining--;
            totalRead++;
            return true;
        }

        /// <summary>
        /// Called when delta token is modified
        /// </summary>
        public void Reset()
        {
            initialSkip = 0;
            totalRead = 0;
        }
    }
}
