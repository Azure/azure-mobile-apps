// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Tracing;
using Moq;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Extensions
{
    public class TraceWriterExtensionsTests
    {
        private readonly string category;
        private readonly string message;
        private readonly Exception exception;
        private readonly HttpRequestMessage request;
        private readonly Mock<ITraceWriter> tracerMock;
        private readonly ITraceWriter tracer;

        public TraceWriterExtensionsTests()
        {
            this.category = "TestCategory";
            this.message = "TestMessage";
            this.exception = new Exception("Catch this!");
            this.request = new HttpRequestMessage();
            this.tracerMock = new Mock<ITraceWriter>();
            this.tracer = this.tracerMock.Object;
        }

        [Fact]
        public void Debug_TracesDebugMessage()
        {
            // Arrange
            TraceRecord record = null;
            this.tracerMock.Setup(t => t.Trace(this.request, this.category, TraceLevel.Debug, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    record = new TraceRecord(req, cat, level);
                    rec(record);
                })
                .Verifiable();

            // Act
            this.tracer.Debug(this.message, this.request, this.category);

            // Assert
            this.tracerMock.Verify();
            this.ValidateTraceRecord(record, this.message);
        }

        [Fact]
        public void Debug_HandlesNullTracer()
        {
            ((ITraceWriter)null).Debug(this.message, this.request, this.category);
        }

        [Fact]
        public void Info_TracesInformationalMessage()
        {
            // Arrange
            TraceRecord record = null;
            this.tracerMock.Setup(t => t.Trace(this.request, this.category, TraceLevel.Info, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    record = new TraceRecord(req, cat, level);
                    rec(record);
                })
                .Verifiable();

            // Act
            this.tracer.Info(this.message, this.request, this.category);

            // Assert
            this.tracerMock.Verify();
            this.ValidateTraceRecord(record, this.message);
        }

        [Fact]
        public void Info_HandlesNullTracer()
        {
            ((ITraceWriter)null).Info(this.message, this.request, this.category);
        }

        [Fact]
        public void Warn_TracesWarningMessage()
        {
            // Arrange
            TraceRecord record = null;
            this.tracerMock.Setup(t => t.Trace(this.request, this.category, TraceLevel.Warn, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    record = new TraceRecord(req, cat, level);
                    rec(record);
                })
                .Verifiable();

            // Act
            this.tracer.Warn(this.message, this.request, this.category);

            // Assert
            this.tracerMock.Verify();
            this.ValidateTraceRecord(record, this.message);
        }

        [Fact]
        public void Warn_HandlesNullTracer()
        {
            ((ITraceWriter)null).Warn(this.message, this.request, this.category);
        }

        [Fact]
        public void Error_TracesException()
        {
            // Arrange
            TraceRecord record = null;
            this.tracerMock.Setup(t => t.Trace(this.request, this.category, TraceLevel.Error, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    record = new TraceRecord(req, cat, level);
                    rec(record);
                })
                .Verifiable();

            // Act
            this.tracer.Error(this.exception, this.request, this.category);

            // Assert
            this.tracerMock.Verify();
            this.ValidateTraceRecord(record, null, this.exception);
        }

        [Fact]
        public void Error_TracesErrorMessage()
        {
            // Arrange
            TraceRecord record = null;
            this.tracerMock.Setup(t => t.Trace(this.request, this.category, TraceLevel.Error, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    record = new TraceRecord(req, cat, level);
                    rec(record);
                })
                .Verifiable();

            // Act
            this.tracer.Error(this.message, this.request, this.category);

            // Assert
            this.tracerMock.Verify();
            this.ValidateTraceRecord(record, this.message);
        }

        [Fact]
        public void Error_TracesExceptionErrorMessage()
        {
            // Arrange
            TraceRecord record = null;
            this.tracerMock.Setup(t => t.Trace(this.request, this.category, TraceLevel.Error, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    record = new TraceRecord(req, cat, level);
                    rec(record);
                })
                .Verifiable();

            // Act
            this.tracer.Error(this.message, this.exception, this.request, this.category);

            // Assert
            this.tracerMock.Verify();
            this.ValidateTraceRecord(record, this.message, this.exception);
        }

        [Fact]
        public void Error_HandlesNullTracer()
        {
            ((ITraceWriter)null).Error(this.message, this.request, this.category);
        }

        [Fact]
        public void Trace_TracesMessage()
        {
            // Arrange
            TraceRecord record = null;
            this.tracerMock.Setup(t => t.Trace(this.request, this.category, TraceLevel.Error, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    record = new TraceRecord(req, cat, level);
                    rec(record);
                })
                .Verifiable();

            // Act
            this.tracer.Trace(TraceLevel.Error, this.message, this.exception, this.request, this.category);

            // Assert
            this.tracerMock.Verify();
            this.ValidateTraceRecord(record, this.message, this.exception);
        }

        [Fact]
        public void Trace_HandlesNullTracer()
        {
            ((ITraceWriter)null).Trace(TraceLevel.Error, this.message, null, this.request, this.category);
        }

        private void ValidateTraceRecord(TraceRecord record, string expectedMessage, Exception expectedException = null)
        {
            Assert.Equal(expectedMessage, record.Message);
            Assert.Equal(expectedException, record.Exception);
        }
    }
}
