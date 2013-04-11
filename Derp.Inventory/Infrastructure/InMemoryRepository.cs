using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Derp.Inventory.Infrastructure
{
    public class InMemoryRepository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly FixedSizedQueue<Guid> commits = new FixedSizedQueue<Guid>(500);
        private readonly ConcurrentDictionary<Guid, List<Event>> storage;

        public InMemoryRepository(ConcurrentDictionary<Guid, List<Event>> storage)
        {
            this.storage = storage;
        }

        #region IRepository<T> Members

        public T GetById(Guid id, int version = Int32.MaxValue)
        {
            List<Event> history;
            if (false == storage.TryGetValue(id, out history)) return null;

            var aggregate = (T) Activator.CreateInstance(typeof (T), true);
            aggregate.LoadsFromHistory(history.Take(version));
            return aggregate;
        }

        public void Save(T aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders = null)
        {
            if (commits.Contains(commitId)) return; // ignore duplicate commi9ts

            commits.Enqueue(commitId);
            var events = aggregate.GetUncommittedChanges();

            var history = storage.AddOrUpdate(aggregate.Id, _ => new List<Event>(), (_, list) => list);

            if (history.Count > aggregate.Version) throw new ConcurrencyException();

            history.AddRange(events);

            aggregate.MarkChangesAsCommitted();
        }

        #endregion
    }
}