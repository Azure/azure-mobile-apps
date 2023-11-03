// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace Datasync.Common;

/// <summary>
/// Provides an ILoggerProvider for testing purposes.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper _output;
    private readonly string[] _categories;

    public TestLoggerFactory(ITestOutputHelper output)
    {
        _output = output;
        _categories = Array.Empty<string>();
    }

    public TestLoggerFactory(ITestOutputHelper output, IEnumerable<string> categories)
    {
        _output = output;
        _categories = categories.ToArray();
    }

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotImplementedException();
    }

    public ILogger CreateLogger(string categoryName)
        => new TestLogger(categoryName, _categories, _output);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    class TestLogger : ILogger
    {
        string _categoryName;
        string[] _categories;
        ITestOutputHelper _output;

        public TestLogger(string categoryName, string[] categories, ITestOutputHelper output)
        {
            _categoryName = categoryName;
            _categories = categories; ;
            _output = output;
        }

        public bool IsEnabled(LogLevel logLevel) => _categories.Contains(_categoryName);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _output.WriteLine(formatter(state, exception));
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
