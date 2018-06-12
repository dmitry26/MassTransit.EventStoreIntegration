using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Dmo.Extensions.MassTransit
{
	public static class MassTransitExts
	{
		public static IRabbitMqHost CreateHost(this IRabbitMqBusFactoryConfigurator cfg,RabbitMqHostOptions opts)
		{
			string host = opts.Host ?? "[::1]";
			ushort port = (ushort)(opts.Port ?? 5672);
			var vhost = string.IsNullOrWhiteSpace(opts.VirtualHost) ? "/" : opts.VirtualHost.Trim('/');

			return cfg.Host(host,port,vhost,null,h =>
			{
				h.Username(opts.Username ?? "guest");
				h.Password(opts.Password ?? "guest");
				h.Heartbeat(opts.Heartbeat ?? 0);

				if (!string.IsNullOrEmpty(opts.ClusterNodeHostname))
				{
					h.UseCluster(cc =>
					{
						cc.Node(opts.ClusterNodeHostname);
						cc.ClusterMembers = opts.ClusterMembers?.Split(',') ?? new string[0];
					});
				}
			});
		}

		public static Task<ConsumeContext<T>> SubscribeHandler<T>(this IBus bus,TimeSpan? timeout = null,CancellationToken cancelToken = default)
		   where T : class
		{
			if (bus == null) throw new ArgumentNullException(nameof(bus));

			if (timeout.HasValue && timeout < Timeout.InfiniteTimeSpan)
				throw new ArgumentOutOfRangeException(nameof(timeout));

			ConnectHandle handler = null;

			var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
			var lnkCancelToken = cts.Token;

			var tcs = new TaskCompletionSource<ConsumeContext<T>>();

			if (timeout.HasValue && timeout.Value != Timeout.InfiniteTimeSpan)
				cts.CancelAfter((new TimeSpan[] { timeout.Value,TimeSpan.FromMilliseconds(1000) }).Max());

			handler = bus.ConnectHandler<T>(context =>
			{
				cts.Dispose();
				if (tcs.TrySetResult(context)) handler.Disconnect();
				return Task.CompletedTask;
			});

			if (!tcs.Task.IsCompleted)
			{
				lnkCancelToken.Register(() =>
				{
					cts.Dispose();
					if (tcs.TrySetCanceled()) handler.Disconnect();
				});
			}

			return tcs.Task;
		}
	}
}
