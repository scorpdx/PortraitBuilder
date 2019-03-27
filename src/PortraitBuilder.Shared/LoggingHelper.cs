using System;
using Microsoft.Extensions.Logging;

namespace PortraitBuilder
{
    public static class LoggingHelper
    {
        /// <summary>
        /// A no-op logger implementation, used when a LoggerFactory is not supplied
        /// </summary>
        public class DummyLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }

        private static readonly Lazy<DummyLogger> dummy = new Lazy<DummyLogger>();

        public static ILoggerFactory LoggerFactory { get; set; }
        public static ILogger CreateLogger<T>() => (ILogger)LoggerFactory?.CreateLogger<T>() ?? dummy.Value;
        public static ILogger CreateLogger(string categoryName) => LoggerFactory?.CreateLogger(categoryName) ?? dummy.Value;
    }
}
