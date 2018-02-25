using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Automatonymous;
using Automatonymous.Contexts;
using Serilog;

namespace MassTransit.EventStoreIntegration.Sample
{
    public class SampleStateMachine : MassTransitStateMachine<SampleInstance>
    {
		private static readonly ILogger _logger = Log.ForContext(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public SampleStateMachine(bool allowSetCompleted = true)
		{
			InstanceState(x => x.CurrentState);

			Event(() => Started,x => x.CorrelateById(e => e.Message.CorrelationId));
			Event(() => Stopped,x => x.CorrelateById(e => e.Message.CorrelationId));
			Event(() => StatusChanged,x => x.CorrelateById(e => e.Message.CorrelationId));

			Initially(
				When(Started)
					.Then(c =>
					{
						_logger.Debug("STATE: Event = {name}, {@data}, Current state: {@instance}",c.Event.Name,c.Data,c.Instance);
						c.Instance.Apply(c.Data);
					})
                    .TransitionTo(Running));

			this.OnUnhandledEvent((ctx) =>
			{
				_logger.Debug($"STATE: Unhandled Event, Current state: {ctx.Instance.CurrentState ?? "null"}, Event = {ctx.Event.Name}");
				return Task.CompletedTask;
			});

			During(Running,
				When(StatusChanged)
                    .Then(c =>
					{
						_logger.Debug("STATE: Event = {name}, {@data}, Current state: {@instance}",c.Event.Name,c.Data,c.Instance);
						c.Instance.Apply(c.Data);
					}),
                When(Stopped)
					.Then(c => _logger.Debug("STATE: Event = {name}, {@data}, Current state: {@instance}",c.Event.Name,c.Data,c.Instance))
					.TransitionTo(Done)
                    .Finalize());

			BeforeEnterAny(x => x.Then(c =>
			{
				_logger.Debug("STATE: Event = {name}, Current state: {@instance}",c.Event.Name,c.Instance);
			}));

			if (allowSetCompleted)
				SetCompletedWhenFinalized();
		}

		public State Running { get; private set; }
        public State Done { get; private set; }
		public Event<ProcessStarted> Started { get; private set; }
		public Event<ProcessStopped> Stopped { get; private set; }
        public Event<OrderStatusChanged> StatusChanged { get; private set; }
    }
}