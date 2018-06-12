using System;
using System.IO;
using EventStore.ClientAPI;
using MassTransit.Serialization;
using Newtonsoft.Json;

namespace MassTransit.EventStoreIntegration
{
	public static class JsonSerialisation
	{
		static readonly Lazy<JsonSerializer> _deserializer;

		static readonly Lazy<JsonSerializer> _serializer;

		static JsonSerialisation()
		{
			_deserializer = new Lazy<JsonSerializer>(() => JsonSerializer.Create(JsonMessageSerializer.DeserializerSettings));
			_serializer = new Lazy<JsonSerializer>(() => JsonSerializer.Create(JsonMessageSerializer.SerializerSettings));
		}

		private static JsonSerializer Deserializer => _deserializer.Value;

		private static JsonSerializer Serializer => _serializer.Value;

		public static object Deserialize(ResolvedEvent resolvedEvent,string assemblyName)
		{
			var type = TypeMapping.Get((resolvedEvent.Event
				?? throw new InvalidOperationException($"{nameof(resolvedEvent.Event)} is null"))
				.EventType,assemblyName);

			using (var stream = new MemoryStream(resolvedEvent.Event.Data))
			{
				using (var reader = new StreamReader(stream))
				{
					return Deserializer.Deserialize(reader,type);
				}
			}
		}

		public static byte[] Serialize(object @event)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					Serializer.Serialize(writer,@event);
				}

				return stream.ToArray();
			}
		}
	}
}
