// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// An exception type representing an error in parsing an OData expression.
    /// </summary>
    public class ODataException : InvalidOperationException
    {
        /// <summary>
        /// Creates a new <see cref="ODataException"/>.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="expression">The expression being parsed.</param>
        /// <param name="errorPosition">The position of the error.</param>
        public ODataException(string message, string expression, int errorPosition)
            : base(message)
        {
            Expression = expression;
            ErrorPosition = errorPosition;
        }

        /// <summary>
        /// Creates a new <see cref="ODataException"/>.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        /// <param name="expression">The expression being parsed.</param>
        /// <param name="errorPosition">The position of the error.</param>
        public ODataException(string message, Exception inner, string expression, int errorPosition)
            : base(message, inner)
        {
            Expression = expression;
            ErrorPosition = errorPosition;
        }

        /// <summary>
        /// The position of the error.
        /// </summary>
        public int ErrorPosition { get; }

        /// <summary>
        /// The OData expression
        /// </summary>
        public string Expression { get; }
    }
}
