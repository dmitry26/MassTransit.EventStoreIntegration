using System;
using Microsoft.Extensions.Logging;

namespace EventStore.Extensions.Logging
{
	/// <summary>
	/// EventStore client logging adapter.
	/// </summary>
	public class EventStoreNetCoreLogger : ClientAPI.ILogger
	{
        private readonly ILogger _logger;       

        public EventStoreNetCoreLogger(ILoggerFactory loggerFactory)
        {
			_logger = loggerFactory.CreateLogger("EventStore.ClientAPI");
        }

        /// <summary>
        /// Writes an error to the logger
        /// </summary>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Error(string format, params object[] args)
        {
            _logger.LogError(format, args);
        }

        /// <summary>
        /// Writes an error to the logger
        /// </summary>
        /// <param name="ex">A thrown exception.</param>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Error(Exception ex, string format, params object[] args)
        {
            _logger.LogError(ex, format, args);
        }

        /// <summary>
        /// Writes an information message to the logger
        /// </summary>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Info(string format, params object[] args)
        {
            _logger.LogInformation(format, args);
        }

        /// <summary>
        /// Writes an information message to the logger
        /// </summary>
        /// <param name="ex">A thrown exception.</param>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Info(Exception ex, string format, params object[] args)
        {
            _logger.LogInformation(ex, format, args);
        }

        /// <summary>
        /// Writes a debug message to the logger
        /// </summary>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Debug(string format, params object[] args)
        {
            _logger.LogDebug(format, args);
        }

        /// <summary>
        /// Writes a debug message to the logger
        /// </summary>
        /// <param name="ex">A thrown exception.</param>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Debug(Exception ex, string format, params object[] args)
        {
            _logger.LogDebug(ex, format, args);
        }
    }
}
