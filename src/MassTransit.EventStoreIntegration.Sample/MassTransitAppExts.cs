using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dmo.Extensions.EventStore;
using Dmo.Extensions.MassTransit;
using EventStore.ClientAPI;
using EventStore.Extensions.Logging;
using GreenPipes;
using MassTransit.EventStoreIntegration.Audit;
using MassTransit.EventStoreIntegration.Saga;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;


namespace MassTransit.EventStoreIntegration.Sample
{
	public static class MassTransitAppExts
	{
		public static void AddMassTransitWithRabbitMq(this IServiceCollection services,IConfiguration appConfig)
		{
			if (services == null)
				throw new ArgumentNullException("services");

			if (appConfig == null)
				throw new ArgumentNullException("appConfig");

			var cfgSection = appConfig.GetSection("RabbitMqHost");

			if (!cfgSection.Exists())
				throw new InvalidOperationException("Appsettings: 'RabbitMqHost' section is not found");

			services.Configure<RabbitMqHostOptions>(cfgSection);

			cfgSection = appConfig.GetSection("AuditStore");

			if (!cfgSection.Exists())
				throw new InvalidOperationException("Appsettings: 'AuditStore' section is not found");

			services.Configure<AuditStoreSettings>(cfgSection);

			services.AddSingleton(svcProv =>
			{
				var conStr = appConfig.GetConnectionString("EvtStoreConnection");
				return EventStoreConnection.Create(conStr,ConnectionSettings.Create()
					.UseNetCoreLogger(svcProv.GetService<ILoggerFactory>())
				//.EnableVerboseLogging()
				);
			});

			services.AddMassTransit(cfg =>
			{
				cfg.AddSaga<SampleInstance>();
			});

			services.AddSingleton(svcProv =>
			{
				var hostOpts = svcProv.GetService<IOptions<RabbitMqHostOptions>>().Value;
				var machine = new SampleStateMachine(svcProv.GetService<ILogger<SampleStateMachine>>(),
					appConfig.GetValue("DeleteCompleted",true));
				var con = svcProv.GetService<IEventStoreConnection>();
				var repository = new EventStoreSagaRepository<SampleInstance>(con);
				var queueName = appConfig.GetValue("SagaQueueName","essaga_test");

				var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
				{
					var host = cfg.CreateHost(hostOpts);

					cfg.ReceiveEndpoint(host,queueName,e =>
					{
						e.UseInMemoryOutbox();
						e.PrefetchCount = 1;
						e.UseConcurrencyLimit(1);
						e.StateMachineSaga(machine,repository);
					});

					cfg.UseSerilog();
				});

				var auditSettings = svcProv.GetService<IOptions<AuditStoreSettings>>().Value;

				if (!string.IsNullOrEmpty(auditSettings?.StreamName))
				{
					var auditStore = new EventStoreMessageAudit(con,auditSettings.StreamName);
					bus.ConnectSendAuditObservers(auditStore);
					bus.ConnectConsumeAuditObserver(auditStore);
				}

				return bus;
			});
		}

		private class AuditStoreSettings
		{
			public string StreamName { get; set; }

			public int MaxAgeSec { get; set; }
		}

		public static IServiceProvider UseMassTransit(this IServiceProvider services)
		{
			var appLifetime = (services ?? throw new ArgumentNullException(nameof(services))).GetService<IApplicationLifetime>();

			Start(services).GetAwaiter().GetResult();

			appLifetime.ApplicationStopped.Register(() =>
			{
				Stop(services);
			});

			return services;
		}

		private static async Task Start(IServiceProvider services)
		{
			var con = services.GetService<IEventStoreConnection>();
			await con.ConnectAsync();

			var auditSettings = services.GetService<IOptions<AuditStoreSettings>>().Value;

			if (auditSettings != null && auditSettings.MaxAgeSec > 0)
				await con.SetEventMaxAgeIfNull(auditSettings.StreamName,TimeSpan.FromSeconds(auditSettings.MaxAgeSec));

			var bus = services.GetService<IBusControl>();
			await bus.StartAsync();
		}

		private static void Stop(IServiceProvider services)
		{
			var bus = services.GetService<IBusControl>();
			bus.Stop();

			var con = services.GetService<IEventStoreConnection>();
			con.Close();
		}
	}
}
