using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransit.EventStoreIntegration.Sample
{
	public class SampleService : BackgroundService
	{
		public SampleService(IBusControl bus,ILogger<SampleService> logger)
		{
			_bus = bus;
			_logger = logger;
		}

		private readonly ILogger _logger;

		private readonly IBus _bus;

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("MyService is starting.");

			var sagaId = Guid.NewGuid();

			await _bus.Publish(new ProcessStarted { CorrelationId = sagaId,OrderId = "321" });

			await _bus.Publish(new OrderStatusChanged { CorrelationId = sagaId,OrderStatus = "Pending" });

			await _bus.Publish(new ProcessStopped { CorrelationId = sagaId });

			_logger.LogInformation("MyService is stopping.");
		}
	}
}
