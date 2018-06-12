using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmo.Extensions.MassTransit;
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
			_logger.LogInformation("SampleService is starting.");

			var sagaId = Guid.NewGuid();

			await _bus.Publish(new ProcessStarted { CorrelationId = sagaId,OrderId = "321" });

			var receivedOrderUpdated = _bus.SubscribeHandler<OrderStatusUpdated>(TimeSpan.FromSeconds(10));

			await _bus.Publish(new OrderStatusChanged { CorrelationId = sagaId,OrderStatus = "Pending" },ctx => ctx.ResponseAddress = _bus.Address);

			try
			{
				await receivedOrderUpdated;

				await _bus.Publish(new ProcessStopped { CorrelationId = sagaId });

				_logger.LogInformation("Published all messages.");
			}
			catch (TaskCanceledException x)
			{
				_logger.LogDebug(x,"Didn't receive a response.");
			}
			catch (Exception x)
			{
				_logger.LogDebug(x,"");
			}
		}
	}
}
