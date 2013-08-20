using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Derp.Inventory.Web.Infrastructure.GetEventStore
{
    public static class GetEventStoreExtensions
    {
        internal static IPEndPoint ParseEndpoint(this string endpoint)
        {
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentException("endpoint");

            var parts = endpoint.Split(':');

            if (parts.Length != 2) throw new ArgumentException("endpoint");

            return new IPEndPoint(IPAddress.Parse(parts[0]), Int32.Parse(parts[1]));
        }

        public static async Task<IList<EventData>> PrepareCommitAsync(
            this IEnumerable<Event> events, Func<Event, Task<EventData>> buildEventData)
        {
            var eventData = new List<EventData>();

            foreach (var @event in events)
            {
                var eventDatum = await buildEventData(@event)
                    .ConfigureAwait(false);

                eventData.Add(eventDatum);
            }

            return eventData;
        }

        public static async Task<IList<Event>> ReadEventsAsync(
            this IEventStoreConnection connection, string id, int version,
            JsonSerializerSettings serializerSettings, int pageSize = 512)
        {
            var position = 0;
            var lastEventVersion = 0;

            var stream = new List<Event>();

            while (lastEventVersion <= version)
            {
                var slice = await connection.ReadStreamEventsForwardAsync(id, position, pageSize, true)
                                                      .ConfigureAwait(false);
                var recordedEvents = slice.Events.Select(x => x.Event);
                foreach (var recordedEvent in recordedEvents)
                {
                    position++;

                    if (recordedEvent.EventType.StartsWith("$"))
                        continue;

                    var @event = await recordedEvent.DeserializeEventAsync(serializerSettings)
                                                    .ConfigureAwait(false);

                    stream.Add((Event)@event);

                    if (++lastEventVersion > version)
                        return stream;
                }

                if (slice.IsEndOfStream)
                    return stream;
            }

            return stream;
        }

        public static async Task<object> DeserializeEventAsync(
            this RecordedEvent recordedEvent, JsonSerializerSettings serializerSettings)
        {
            var headers = await recordedEvent.DeserializeHeadersAsync(serializerSettings)
                                             .ConfigureAwait(false);

            object typeName;
            if (headers == null || false == headers.TryGetValue(GetEventStoreHeaders.Type, out typeName))
                return null;

            var type = Type.GetType((String) typeName);

            return await recordedEvent.Data.DeserializeEventAsync(type, serializerSettings)
                                      .ConfigureAwait(false);
        }

        public static async Task<object> DeserializeEventAsync(
            this byte[] data, Type type, JsonSerializerSettings serializerSettings)
        {
            return await JsonConvert.DeserializeObjectAsync(
                Encoding.UTF8.GetString(data),
                type,
                serializerSettings)
                                    .ConfigureAwait(false);
        }

        public static async Task<Dictionary<string, object>> DeserializeHeadersAsync(
            this RecordedEvent recordedEvent, JsonSerializerSettings serializerSettings)
        {
            return await JsonConvert.DeserializeObjectAsync<Dictionary<string, object>>(
                Encoding.UTF8.GetString(recordedEvent.Metadata),
                serializerSettings)
                                    .ConfigureAwait(false);
        }

        public static async Task PersistCommitAsync(
            this IEventStoreConnection connection, Commit commit, int pageSize = 512)
        {
            var expectedVersion = commit.Version - commit.Events.Count();

            var expectedEventStreamVersion = expectedVersion == 0
                ? ExpectedVersion.NoStream
                : expectedVersion;

            if (commit.Events.Count < pageSize)
            {
                await connection.AppendToStreamAsync(commit.StreamId, expectedEventStreamVersion, commit.Events)
                                .ConfigureAwait(false);
            }
            else
            {
                var transaction = await connection.StartTransactionAsync(commit.StreamId, expectedEventStreamVersion)
                                                  .ConfigureAwait(false);

                for (var i = 0; i < commit.Events.Count; i += pageSize)
                {
                    await transaction.WriteAsync(commit.Events.Skip(i).Take(pageSize))
                                     .ConfigureAwait(false);
                }

                await transaction.CommitAsync()
                                 .ConfigureAwait(false);
            }
        }

        public static async Task<EventData> CreateEventDataAsync(
            this Event @event, Guid eventId, JsonSerializerSettings jsonSerializerSettings, bool dispatchable = true,
            Action<IDictionary<string, object>> updateHeaders = null)
        {
            var headers = new Dictionary<string, object>
            {
                {
                    GetEventStoreHeaders.Type, @event.GetType().ToPartiallyQualifiedName()
                },
                {
                    GetEventStoreHeaders.Timestamp, DateTime.UtcNow
                }
            };
            updateHeaders = updateHeaders ?? (_ => { });
            updateHeaders(headers);

            var body = await JsonConvert.SerializeObjectAsync(@event, Formatting.Indented, jsonSerializerSettings)
                .ConfigureAwait(false);
            var metadata = await JsonConvert.SerializeObjectAsync(headers, Formatting.Indented, jsonSerializerSettings)
                .ConfigureAwait(false);

            return new EventData(
                eventId, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(body), Encoding.UTF8.GetBytes(metadata));
        }
    }
}