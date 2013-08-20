using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derp.Inventory.Infrastructure;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Derp.Inventory.Web.GetEventStore
{
    public class GetEventStoreRepository<TAggregate> : IRepository<TAggregate> where TAggregate : AggregateRoot
    {
        private const int PageSize = 512;
        private readonly EventStoreConnection connection;
        private readonly Func<Guid, string> getStreamId;
        private readonly JsonSerializerSettings serializerSettings;

        /// <summary>
        /// Creates a the repoisory
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="boundedContext">The name of the bounded context.  Typically an aggregate crosses context boundaries, e.g. a visitor in the web context becomes a customer in the sales context. </param>
        /// <param name="customizeSerailzer">customize the json. You should probably not use this.</param>
        /// <param name="upconversion">optional event converter</param>
        public GetEventStoreRepository(EventStoreConnection connection, string boundedContext,
                                       Action<JsonSerializerSettings> customizeSerailzer = null)
            : this(connection, id => boundedContext + "-" + id.ToString("n"), customizeSerailzer ?? (s => { }))
        {
        }

        public GetEventStoreRepository(
            EventStoreConnection connection, Func<Guid, string> getStreamId,
            Action<JsonSerializerSettings> customizeSerializer)
        {
            this.connection = connection;

            this.getStreamId = getStreamId;

            serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
            };
            customizeSerializer(serializerSettings);
        }

        #region IRepository<TAggregate> Members

        public TAggregate GetById(Guid id, int version = Int32.MaxValue)
        {
            return GetByIdAsync(id, version).Result;
        }

        public void Save(TAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders = null)
        {
            SaveAsync(aggregate, commitId, updateHeaders).Wait();
        }

        #endregion

        public async Task<TAggregate> GetByIdAsync(Guid id, int version = Int32.MaxValue)
        {
            var aggregate = (TAggregate) Activator.CreateInstance(typeof (TAggregate), true);

            IEnumerable<Event> stream = await connection.ReadEventsAsync(getStreamId(id), version, serializerSettings).ConfigureAwait(false);

            aggregate.LoadsFromHistory(stream);

            return aggregate;
        }

        public async Task SaveAsync(TAggregate aggregate, Guid commitId,
                                    Action<IDictionary<string, object>> updateHeaders = null)
        {
            var changes = aggregate.GetUncommittedChanges().ToList();

            var version = aggregate.Version - changes.Count;

            var events = await changes.PrepareCommitAsync(
                async e => await e.CreateEventDataAsync(
                    DeterministicGuid.CreateFrom(commitId, ++version),
                    serializerSettings, updateHeaders: updateHeaders)).ConfigureAwait(false);

            await connection.PersistCommitAsync(new Commit(getStreamId(aggregate.Id), version, events)).ConfigureAwait(false);
        }
    }
}