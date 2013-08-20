using System;
using System.Collections.Generic;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Projections.InMemory
{
    public class InMemoryViewWriter<TKey, TView> : IWriteViews<TKey, TView>,
                                                   IGetEventStorePositionRepository
        where TView : class
    {
        private readonly IDictionary<TKey, TView> storage;
        private Position lastProcessed;

        public InMemoryViewWriter(IDictionary<TKey, TView> storage)
        {
            this.storage = storage;
        }

        #region IGetEventStorePositionRepository Members

        public Position GetLastProcessedPosition()
        {
            return lastProcessed;
        }

        #endregion

        #region IWriteViews<TKey,TView> Members

        public void AddOrUpdate(TKey key, Position position, Func<TView> add, Action<TView> update = null)
        {
            TView view;
            if (false == storage.TryGetValue(key, out view))
            {
                view = AddInternal(key, add);
            }
            if (update != null)
            {
                update(view);
            }
            lastProcessed = position;
        }

        public void TryUpdate(TKey key, Position position, Action<TView> update)
        {
            TView view;
            if (false == storage.TryGetValue(key, out view))
            {
                return;
            }

            update(view);
        }

        public void TryDelete(TKey key, Position position)
        {
            TView view;
            if (false == storage.TryGetValue(key, out view))
            {
                return;
            }
            storage.Remove(key);
        }

        #endregion

        private TView AddInternal(TKey key, Func<TView> add)
        {
            var view = add();
            storage.Add(key, view);
            return view;
        }
    }
}