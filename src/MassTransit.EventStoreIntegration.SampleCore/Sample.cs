using System;
using System.Configuration;
using System.Threading.Tasks;
using Automatonymous;
using Dmo.NetCore.Hosting;
using EventStore.ClientAPI;
using EventStore.SerilogAdapter;
using GreenPipes;
using MassTransit;
using MassTransit.EventStoreIntegration.Audit;
using MassTransit.EventStoreIntegration.Saga;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MassTransit.EventStoreIntegration.Sample
{
	public class Sample
	{
		private IBusControl _bus;
		private IEventStoreConnection _connection;
		private EventStoreMessageAudit _auditStore;
		private AuditStoreSettings _auditSettings;

		private static readonly Serilog.ILogger _logger = Log.ForContext(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public Sample()
		{
			var appSettings = ConfigExts.GetAppSettings();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(appSettings)
				.CreateLogger();

			var conStr = appSettings.GetConnectionString("EvtStoreConnection");

			if (string.IsNullOrEmpty(conStr))
				throw new InvalidOperationException("conStr is null or empty.");

			_connection = EventStoreConnection.Create(conStr,ConnectionSettings.Create().UseSerilog());

			var repository = new EventStoreSagaRepository<SampleInstance>(_connection);
			_bus = Bus.Factory.CreateUsingRabbitMq(c =>
			{
				c.UseSerilog();

				var host = c.Host(new Uri("rabbitmq://localhost"),h =>
			   {
				   h.Username("guest");
				   h.Password("guest");
			   });

				var machine = new SampleStateMachine(appSettings.GetValue("DeleteCompleted",true));

				c.ReceiveEndpoint(host,"essaga_test",ep =>
			  {
				  ep.UseInMemoryOutbox();
				  ep.UseConcurrencyLimit(1);
				  ep.PrefetchCount = 1;
				  ep.StateMachineSaga(machine,repository);
			  });
			});

			_auditSettings = appSettings.GetSettings<AuditStoreSettings>("AuditStore");

			if (!string.IsNullOrEmpty(_auditSettings?.StreamName))
			{
				_auditStore = new EventStoreMessageAudit(_connection,_auditSettings.StreamName);
				_bus.ConnectSendAuditObservers(_auditStore);
				_bus.ConnectConsumeAuditObserver(_auditStore);
			}
		}

		public async Task Execute()
		{
			await _connection.ConnectAsync();

			if (_auditSettings != null && _auditSettings.MaxAgeSec > 0)
				await _connection.SetEventMaxAgeIfNull(_auditSettings.StreamName,TimeSpan.FromSeconds(_auditSettings.MaxAgeSec));

			await _bus.StartAsync();

			var sagaId = Guid.NewGuid();

			await _bus.Publish<ProcessStarted>(new ProcessStarted { CorrelationId = sagaId,OrderId = "321" });

			await _bus.Publish(new OrderStatusChanged { CorrelationId = sagaId,OrderStatus = "Pending" });

			await _bus.Publish(new ProcessStopped { CorrelationId = sagaId });
		}

		public void Stop()
		{
			_bus.Stop();
			_connection.Dispose();
		}

		private class AuditStoreSettings
		{
			public string StreamName { get; set; }

			public int MaxAgeSec { get; set; }
		}
	}
}