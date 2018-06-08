using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dmo.Extensions.Configuration
{
	public static class ConfigurationExts
	{
		public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder bldr,IHostingEnvironment env,string[] args = null,bool optional = true,bool reloadOnChange = false)
		{
			var envName = env.EnvironmentName;

			bldr.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json",optional,reloadOnChange)
				.AddJsonFile($"appsettings.{envName}.json",optional,reloadOnChange);

			if (env.IsDevelopment())
				bldr.AddUserSecrets(System.Reflection.Assembly.GetEntryAssembly(),optional: true);

			var fargs = WithoutShortSwitches(args);

			if (fargs != null && fargs.Length > 0)
				bldr.AddCommandLine(fargs);

			return bldr;
		}

		private static string[] WithoutShortSwitches(IEnumerable<string> args)
		{
			return args?.Where(x => x.StartsWith("--") || x.StartsWith("/")).ToArray();
		}

		public static T GetSettings<T>(this IConfiguration config,string sectionKey = null)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			return string.IsNullOrEmpty(sectionKey) ? config.Get<T>() : config.GetSection(sectionKey).Get<T>();
		}

		public static object GetSettings(this IConfiguration config,Type type,string sectionKey = null)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (type == null)
				throw new ArgumentNullException("type");

			return string.IsNullOrEmpty(sectionKey) ? config.Get(type) : config.GetSection(sectionKey).Get(type);
		}
	}
}
