using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dmo.Extensions.Hosting
{
	public static class HostingHostBuilderExts
	{
		public static Task RunConsoleAsync(this IHostBuilder hostBuilder,Action<IServiceProvider> cfgApp,CancellationToken cancellationToken = default)
		{
			if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

			var host = (hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder)))
				.UseConsoleLifetime().Build();

			cfgApp?.Invoke(host.Services);

			return host.RunAsync(cancellationToken);
		}

		public static IHostBuilder UseEnvironment(this IHostBuilder hostBuilder,string[] args)
		{
			return hostBuilder.ConfigureHostConfiguration(configBuilder =>
			{
				configBuilder.AddEnvironmentVariables("DOTNETCORE_");

				if (args != null && args.Length > 0)
					configBuilder.AddCommandLine(GetArgs(args,x => x),GetSwitchMappings());
			});
		}

		private static Dictionary<string,string> GetSwitchMappings() => new Dictionary<string,string>
		{
			{"--urls","urls"},
			{"--environment","environment"},
			{"-u","urls"},
			{"-e","environment"},
		};

		private static IConfigurationRoot BuildEnvArgConfig(string[] args)
		{
			if (args == null || args.Length == 0)
				return new ConfigurationBuilder().Build();

			var res = new ConfigurationBuilder()
				.AddCommandLine(GetArgs(args,x => x),GetSwitchMappings())
				.Build();

			return res;
		}

		private static string[] GetArgs(string[] args,Func<bool,bool> predicate)
		{
			var map = GetSwitchMappings();

			var items = args.Select(x =>
			{
				var idx = x.IndexOf('=');
				if (x[0] == '/') x = (idx == 2 ? "-" : "--") + x.Substring(1);
				return new { Arg = x,Key = (idx > 0) ? x.Substring(0,idx) : x };
			});

			return items.Where(x => predicate(map.ContainsKey(x.Key))).Select(x => x.Arg).ToArray();
		}
	}
}
