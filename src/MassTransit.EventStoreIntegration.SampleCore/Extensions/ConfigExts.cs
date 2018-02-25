using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Dmo.NetCore.Hosting
{
	public static class ConfigExts
	{		
		public static IConfigurationRoot ReadJsonConfigFile(string filename,bool optional = false,bool reloadOnChange = false)
		{
			return new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile(filename,optional,reloadOnChange)			
			.Build();			
		}

		public static IConfigurationRoot GetAppSettings(string[] args=null,bool optional = true,bool reloadOnChange = false)
		{
			var hostEnv = GetHostEnvironment();

			var bldr = new ConfigurationBuilder()
				.AddEnvironmentVariables("DOTNETCORE_")
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json",optional,reloadOnChange)
				.AddJsonFile($"appsettings.{hostEnv}.json",optional,reloadOnChange);

			if (IsDevelopment(hostEnv))			
				bldr.AddUserSecrets(Assembly.GetEntryAssembly(),optional:true);			

			if (args != null && args.Length > 0)
				bldr.AddCommandLine(args);

			return bldr.Build();
		}

		private static string GetHostEnvironment()
		{			
			return Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT") ?? EnvironmentName.Production;
		}

		private static bool IsEnvironment(string hostEnv,string envName) => string.Equals(hostEnv,envName,StringComparison.OrdinalIgnoreCase);		

		public static bool IsDevelopment(string hostEnv) => IsEnvironment(hostEnv,EnvironmentName.Development);		

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

	public static class EnvironmentName
	{
		public static readonly string Development = "Development";
		public static readonly string Staging = "Staging";
		public static readonly string Production = "Production";
	}
}
