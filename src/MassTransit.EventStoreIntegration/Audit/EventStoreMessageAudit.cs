﻿using System;
using System.IO;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using MassTransit.Audit;
using MassTransit.Util;
using Newtonsoft.Json;

namespace MassTransit.EventStoreIntegration.Audit
{
    public class EventStoreMessageAudit : IMessageAuditStore
    {
        private readonly IEventStoreConnection _connection;

        public string StreamName { get; private set; }

		public EventStoreMessageAudit(IEventStoreConnection connection, string auditStreamName)
        {
            _connection = connection;
			StreamName = auditStreamName;
		}

        public async Task StoreMessage<T>(T message, MessageAuditMetadata metadata) where T : class
        {
            var auditEvent = new EventData(Guid.NewGuid(), TypeMetadataCache<T>.ShortName,
                true, Serialise(message), Serialise(metadata));
            await _connection.AppendToStreamAsync(StreamName, ExpectedVersion.Any, auditEvent)
                .ConfigureAwait(false);
		}

        private static byte[] Serialise(object @event)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    JsonSerializer.CreateDefault().Serialize(writer, @event);
                }
                return stream.ToArray();
            }
        }
    }
}