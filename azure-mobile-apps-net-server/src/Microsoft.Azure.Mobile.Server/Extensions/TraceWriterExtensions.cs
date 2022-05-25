// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web.Http.Tracing;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="ITraceWriter"/> providing easy usable methods for logging.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TraceWriterExtensions
    {
        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Debug"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="message">The message to log.</param>
        /// <param name="request">Request used for message correlation. Defaults to null.</param>
        /// <param name="category">The category for the trace. Defaults to the method or property name of the caller.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameters are in order for log messages.")]
        public static void Debug(this ITraceWriter traceWriter, string message, HttpRequestMessage request = null, [CallerMemberName] string category = "")
        {
            Trace(traceWriter, TraceLevel.Debug, message, null, request, category);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Info"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="message">The message to log.</param>
        /// <param name="request">Request used for message correlation. Defaults to null.</param>
        /// <param name="category">The category for the trace. Defaults to the method or property name of the caller.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameters are in order for log messages.")]
        public static void Info(this ITraceWriter traceWriter, string message, HttpRequestMessage request = null, [CallerMemberName] string category = "")
        {
            Trace(traceWriter, TraceLevel.Info, message, null, request, category);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Warn"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="message">The message to log.</param>
        /// <param name="request">Request used for message correlation. Defaults to null.</param>
        /// <param name="category">The category for the trace. Defaults to the method or property name of the caller.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameters are in order for log messages.")]
        public static void Warn(this ITraceWriter traceWriter, string message, HttpRequestMessage request = null, [CallerMemberName] string category = "")
        {
            Trace(traceWriter, TraceLevel.Warn, message, null, request, category);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Error"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="message">The message to log.</param>
        /// <param name="request">Request used for message correlation. Defaults to null.</param>
        /// <param name="category">The category for the trace. Defaults to the method or property name of the caller.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameters are in order for log messages.")]
        public static void Error(this ITraceWriter traceWriter, string message, HttpRequestMessage request = null, [CallerMemberName] string category = "")
        {
            Trace(traceWriter, TraceLevel.Error, message, null, request, category);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Error"/> with the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="request">Request used for message correlation. Defaults to null.</param>
        /// <param name="category">The category for the trace. Defaults to the method or property name of the caller.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameters are in order for log messages.")]
        public static void Error(this ITraceWriter traceWriter, Exception exception, HttpRequestMessage request = null, [CallerMemberName] string category = "")
        {
            Trace(traceWriter, TraceLevel.Error, null, exception, request, category);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Error"/> with the given message and exception.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="request">Request used for message correlation. Defaults to null.</param>
        /// <param name="category">The category for the trace. Defaults to the method or property name of the caller.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameters are in order for log messages.")]
        public static void Error(this ITraceWriter traceWriter, string message, Exception exception, HttpRequestMessage request = null, [CallerMemberName] string category = "")
        {
            Trace(traceWriter, TraceLevel.Error, message, exception, request, category);
        }

        /// <summary>
        /// Writes a single <see cref="TraceRecord"/> to the given <see cref="ITraceWriter"/> if the trace writer
        /// is enabled for the given <paramref name="category"/> and <paramref name="level"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="level">The <see cref="TraceLevel"/> for the trace.</param>
        /// <param name="message">The message to log (or null).</param>
        /// <param name="exception">The exception to log (or null).</param>
        /// <param name="request">Request used for message correlation. Defaults to null.</param>
        /// <param name="category">The category for the trace. Defaults to the method or property name of the caller.</param>
        public static void Trace(this ITraceWriter traceWriter, TraceLevel level, string message, Exception exception, HttpRequestMessage request = null, [CallerMemberName] string category = "")
        {
            if (traceWriter == null)
            {
                // If tracing is disabled or we don't have a trace writer then just return.
                return;
            }

            traceWriter.Trace(
                request,
                category,
                level,
                (TraceRecord traceRecord) =>
                {
                    traceRecord.Message = message;
                    traceRecord.Exception = exception;
                });
        }
    }
}