﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using MassTransit.EventStoreIntegration.Saga;
using MassTransit.Logging;

namespace MassTransit.EventStoreIntegration
{
    public static class EventStoreExtensions
    {
        static readonly ILog _logger = Logger.Get(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<long> SaveEvents(this IEventStoreConnection connection,
            string streamIdentifier,
            IEnumerable<object> events,
            long expectedVersion,
            EventMetadata metadata)
        {
            var esEvents = events
                .Select(x =>
                    new EventData(
                        Guid.NewGuid(),
                        TypeMapping.GetTypeName(x.GetType()),
                        true,
                        JsonSerialisation.Serialize(x),
                        JsonSerialisation.Serialize(metadata)));

            try
            {
                var result = await connection.AppendToStreamAsync(streamIdentifier,expectedVersion,esEvents);
                _logger.Debug($"Saved events to store: stream = {streamIdentifier}, expectedVersion = {expectedVersion}, NextExpectedVersion = {result.NextExpectedVersion}");
                return result.NextExpectedVersion;
            }
            catch (WrongExpectedVersionException x)
            {
                _logger.Debug(x.Message);
                throw new EventStoreSagaConcurrencyException(x.Message);
            }
        }

        public static async Task<EventsData> ReadEvents(this IEventStoreConnection connection,
            string streamName, int sliceSize, Assembly assembly)
        {
            var slice = await
                connection.ReadStreamEventsForwardAsync(streamName, StreamPosition.Start, sliceSize, false);
            if (slice.Status == SliceReadStatus.StreamDeleted || slice.Status == SliceReadStatus.StreamNotFound)
            {
                _logger.Debug($"Failed to retrieve events: stream = {streamName}, status = {slice.Status}");
                return null;
            }

            var assemblyName = assembly.GetName().Name;
            var lastEventNumber = slice.LastEventNumber;
            var events = new List<object>();
            events.AddRange(slice.Events.Select(x => JsonSerialisation.Deserialize(x, assemblyName)));

            while (!slice.IsEndOfStream)
            {
                slice = await
                    connection.ReadStreamEventsForwardAsync(streamName, slice.NextEventNumber, sliceSize, false);
                events.AddRange(slice.Events.Select(x => JsonSerialisation.Deserialize(x, assemblyName)));
                lastEventNumber = slice.LastEventNumber;
            }
            var tuple = new EventsData(events, lastEventNumber);
            return tuple;
        }
    }
}