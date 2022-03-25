// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// An exception thrown when push does not complete successfully.
    /// </summary>
    [SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.", Justification = "Specialized exception")]
    public class PushFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PushFailedException"/>
        /// </summary>
        /// <param name="pushResult">Result of push operation.</param>
        /// <param name="innerException">Inner exception that caused the push to fail.</param>
        public PushFailedException(PushCompletionResult pushResult, Exception innerException)
            : base("Push operation has failed. See the PushResult for details.", innerException)
        {
            PushResult = pushResult;
        }

        /// <summary>
        /// Result of push operation
        /// </summary>
        public PushCompletionResult PushResult { get; }
    }
}
