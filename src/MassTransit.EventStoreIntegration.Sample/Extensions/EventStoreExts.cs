using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

namespace Dmo.Extensions.EventStore
{
    public static class EventStoreExts
    {
		public static async Task SetEventMaxAgeIfNull(this IEventStoreConnection connection,string streamName,TimeSpan maxAge)
		{
			for (int i = 0; i < 3; ++i)
			{
				var readRes = await connection.GetStreamMetadataAsync(streamName);

				if (readRes.IsStreamDeleted || readRes.StreamMetadata.MaxAge != null)
					return;

				var metadata = StreamMetadata.Create(maxAge: maxAge);

				try
				{
					await connection.SetStreamMetadataAsync(streamName,readRes.MetastreamVersion,metadata);
					return;
				}
				catch (WrongExpectedVersionException)
				{
				}

				await Task.Delay((i + 1) * 100);
			}
		}
	}
}
