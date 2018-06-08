using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Dmo.Extensions.Hosting;
using Dmo.Extensions.Configuration;

namespace MassTransit.EventStoreIntegration.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				Console.Title = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);

				await RunAsync(args);
			}
			catch (Exception x)
			{
				Console.WriteLine(x);
				Console.ReadLine();
			}
		}

		static async Task RunAsync(string[] args)
		{
			var builder = new HostBuilder()
				.UseEnvironment(args)
				.ConfigureAppConfiguration((hostContext,config) =>
				{
					config.AddAppSettings(hostContext.HostingEnvironment,args);
				})
				.ConfigureServices((hostContext,services) =>
				{
					services.AddMassTransitWithRabbitMq(hostContext.Configuration);
					services.AddScoped<IHostedService,SampleService>();
				})
				.UseSerilog((hostContext,config) => config.ReadFrom.Configuration(hostContext.Configuration));

			await builder.RunConsoleAsync(services => services.UseMassTransit());
		}
	}
}
