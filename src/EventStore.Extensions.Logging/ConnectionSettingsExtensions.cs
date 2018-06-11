using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;

namespace EventStore.Extensions.Logging
{
	/// <summary>
	/// Extensions to use on <see cref="ConnectionSettingsBuilder"/>
	/// </summary>
	public static class ConnectionSettingsExtensions
	{
		/// <summary>
		/// Creates a logging adapter for registered ILoggerProviders.
		/// </summary>
		/// <param name="connectionSettingsBuilder">The connection settings builder.</param>
		/// <param name="loggerFactory">The logger factory.</param>
		/// <returns></returns>
		public static ConnectionSettingsBuilder UseNetCoreLogger(this ConnectionSettingsBuilder connectionSettingsBuilder,ILoggerFactory loggerFactory)
		{
			return connectionSettingsBuilder.UseCustomLogger(new EventStoreNetCoreLogger(loggerFactory));
		}
	}
}