using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Derp.Inventory.Web.GetEventStore
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
                var eventDatum = await buildEventData(@event);

                eventData.Add(eventDatum);
            }

            return eventData;
        }

        public static async Task<IList<Event>> ReadEventsAsync(this EventStoreConnection eventStoreConnection, string id,
                                                               int version, JsonSerializerSettings serializerSettings,
                                                               int pageSize = 512)
        {
            var position = 1;
            var lastEventVersion = 0;

            var stream = new List<Event>();

            while (lastEventVersion <= version)
            {
                var slice = await eventStoreConnection.ReadStreamEventsForwardAsync(id, position, pageSize, true);
                foreach (var recordedEvent in slice.Events.Select(x => x.Event))
                {
                    position++;

                    var @event = await recordedEvent.DeserializeEventAsync(serializerSettings);

                    stream.Add((Event) @event);

                    if (++lastEventVersion > version) return stream;
                }

                if (slice.IsEndOfStream) return stream;
            }

            return stream;
        }

        public static async Task<object> DeserializeEventAsync(this RecordedEvent recordedEvent,
                                                               JsonSerializerSettings serializerSettings)
        {
            var headers = await recordedEvent.DeserializeHeadersAsync(serializerSettings);

            object typeName;
            if (headers == null || false == headers.TryGetValue(GetEventStoreHeaders.Type, out typeName))
                return null;

            var type = Type.GetType((String) typeName);

            return await recordedEvent.Data.DeserializeEventAsync(type, serializerSettings);
        }

        public static async Task<object> DeserializeEventAsync(this byte[] data, Type type,
                                                               JsonSerializerSettings serializerSettings)
        {
            return await JsonConvert.DeserializeObjectAsync(
                Encoding.UTF8.GetString(data),
                type,
                serializerSettings);
        }

        public static Task<Dictionary<string, object>> DeserializeHeadersAsync(this RecordedEvent recordedEvent,
                                                                               JsonSerializerSettings serializerSettings)
        {
            return JsonConvert.DeserializeObjectAsync<Dictionary<string, object>>(
                Encoding.UTF8.GetString(recordedEvent.Metadata),
                serializerSettings);
        }

        public static async Task PersistCommitAsync(this EventStoreConnection connection, Commit commit,
                                                    int pageSize = 512)
        {
            var expectedVersion = commit.Version - commit.Events.Count();

            var expectedEventStreamVersion = expectedVersion == 0
                                                 ? ExpectedVersion.NoStream
                                                 : expectedVersion;

            var transaction = await connection.StartTransactionAsync(commit.StreamId, expectedEventStreamVersion);

            for (var i = 0; i < commit.Events.Count; i += pageSize)
            {
                await transaction.WriteAsync(commit.Events.Skip(i).Take(pageSize));
            }

            await transaction.CommitAsync();
        }

        public static async Task<EventData> CreateEventDataAsync(this Event @event, Guid eventId,
                                                                 JsonSerializerSettings jsonSerializerSettings,
                                                                 bool dispatchable = true,
                                                                 Action<IDictionary<string, object>> updateHeaders =
                                                                     null)
        {
            var headers = new Dictionary<string, object>
            {
                {GetEventStoreHeaders.Type, @event.GetType().ToPartiallyQualifiedName()},
                {GetEventStoreHeaders.Timestamp, DateTime.UtcNow},
                {GetEventStoreHeaders.CorrelationId, eventId}
            };
            updateHeaders = updateHeaders ?? (_ => { });
            updateHeaders(headers);

            var body = await JsonConvert.SerializeObjectAsync(@event, Formatting.Indented, jsonSerializerSettings);
            var metadata = await JsonConvert.SerializeObjectAsync(headers, Formatting.Indented, jsonSerializerSettings);

            return new EventData(
                eventId, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(body), Encoding.UTF8.GetBytes(metadata));
        }
    }
}